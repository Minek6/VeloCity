
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Common.Interfaces;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Tickets.Commands.ValidateTicket;

public class ValidateTicketHandler(
    ApplicationDbContext context,
    IUserContext userContext,
    IConfiguration configuration
) : IRequestHandler<ValidateTicketCommand>
{
    public async Task Handle(
        ValidateTicketCommand request,
        CancellationToken ct)
    {
        var userId = userContext.Id
            ?? throw new AppException(
                "Unauthorized access.", 401);

        if (request.TicketId <= 0 || request.VehicleId <= 0)
            throw new AppException(
                "Invalid ticket or vehicle ID.", 400);

        var ticket = await context.Tickets
            .AsNoTracking()
            .Include(t => t.TicketType)
            .FirstOrDefaultAsync(
                t => t.Id == request.TicketId, ct)
            ?? throw new NotFoundException(
                "Ticket", request.TicketId);

        if (ticket.UserId != userId)
            throw new AppException(
                "You do not own this ticket.", 403);

        if (ticket.IsValidated)
            throw new AppException(
                "This ticket has already been validated.", 400);

        var vehicleExists = await context.Vehicles
            .AnyAsync(v =>
                v.Id == request.VehicleId &&
                v.IsActive, ct);

        if (!vehicleExists)
            throw new AppException(
                "Invalid or inactive vehicle.", 400);

        var isDemo =
            configuration["Timetable:Mode"] == "Demo";

        if (ticket.ServiceDate.HasValue && !isDemo)
        {
            // Data lokalna dla komunikacji miejskiej w Polsce.
            var polishTimeZone =
                TimeZoneInfo.FindSystemTimeZoneById(
                    "Europe/Warsaw");

            var localNow = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                polishTimeZone);

            var today = DateOnly.FromDateTime(localNow);

            if (ticket.ServiceDate.Value != today)
                throw new AppException(
                    "Ticket is not for today's service date.", 400);
        }

        var now = DateTime.UtcNow;

        
        var duration = ticket.TicketType.DurationInMinutes;

        if (duration <= 0)
        {
            if (!isDemo)
            {
                throw new AppException(
                    "Single tickets require a configured validity policy.",
                    400);
            }

            duration = configuration.GetValue<int>(
                "Timetable:DemoSingleTicketValidityMinutes");

            if (duration <= 0)
            {
                throw new AppException(
                    "Invalid demo ticket validity configuration.",
                    500);
            }
        }

        DateTime? validTo = now.AddMinutes(duration);

        // Atomowa aktywacja.
        // Tylko pierwsze żądanie może zmienić status biletu.
        var updated = await context.Tickets
            .Where(t =>
                t.Id == request.TicketId &&
                t.UserId == userId &&
                !t.IsValidated)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.IsValidated, true)
                .SetProperty(
                    t => t.VehicleId,
                    (int?)request.VehicleId)
                .SetProperty(t => t.ValidFrom, (DateTime?)now)
                .SetProperty(t => t.ValidTo, validTo),
                ct);

        if (updated != 1)
            throw new AppException(
                "Ticket was already activated.", 409);
    }
}
