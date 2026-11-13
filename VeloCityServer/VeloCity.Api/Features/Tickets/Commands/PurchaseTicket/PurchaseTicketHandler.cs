
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Common.Interfaces;
using VeloCity.Api.Models;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Tickets.Commands.PurchaseTicket;

public class PurchaseTicketHandler(
    ApplicationDbContext context,
    IUserContext userContext,
    IConfiguration configuration
) : IRequestHandler<PurchaseTicketCommand, int>
{
    public async Task<int> Handle(
        PurchaseTicketCommand request,
        CancellationToken ct)
    {
        var userId = userContext.Id
            ?? throw new AppException("Unauthorized access.", 401);

        bool hasTrip = request.ScheduledTripId.HasValue;

        // Walidacja również w handlerze: nie polegamy
        // wyłącznie na uruchomieniu FluentValidation.
        bool hasAllConnectionFields =
            request.ServiceDate.HasValue &&
            request.StartStopId.HasValue &&
            request.EndStopId.HasValue;

        bool hasAnyConnectionFields =
            request.ServiceDate.HasValue ||
            request.StartStopId.HasValue ||
            request.EndStopId.HasValue;

        if ((hasTrip && !hasAllConnectionFields) ||
            (!hasTrip && hasAnyConnectionFields))
        {
            throw new AppException(
                "Incomplete connection information.");
        }

        if (request.TicketTypeId <= 0)
            throw new AppException("Invalid ticket type.");

        if (hasTrip)
        {
            if (request.ScheduledTripId!.Value <= 0 ||
                request.StartStopId!.Value <= 0 ||
                request.EndStopId!.Value <= 0 ||
                request.StartStopId == request.EndStopId)
            {
                throw new AppException(
                    "Invalid connection parameters.");
            }

            var scheduledTrip = await context.ScheduledTrips
                .Include(x => x.ServiceCalendar)
                .FirstOrDefaultAsync(
                    x => x.Id == request.ScheduledTripId.Value,
                    ct);

            if (scheduledTrip is null)
                throw new AppException(
                    "Selected scheduled trip does not exist.");

            var serviceDate = request.ServiceDate!.Value;
            var calendar = scheduledTrip.ServiceCalendar;

            if (serviceDate < calendar.ValidFrom ||
                serviceDate > calendar.ValidTo)
            {
                throw new AppException(
                    "Trip is not available on selected date.");
            }

            bool regularService = serviceDate.DayOfWeek switch
            {
                DayOfWeek.Monday => calendar.Monday,
                DayOfWeek.Tuesday => calendar.Tuesday,
                DayOfWeek.Wednesday => calendar.Wednesday,
                DayOfWeek.Thursday => calendar.Thursday,
                DayOfWeek.Friday => calendar.Friday,
                DayOfWeek.Saturday => calendar.Saturday,
                DayOfWeek.Sunday => calendar.Sunday,
                _ => false
            };

            var exceptions = await context.ServiceCalendarExceptions
                .Where(x =>
                    x.ServiceCalendarId == calendar.Id &&
                    x.Date == serviceDate)
                .Select(x => x.IsAdded)
                .ToListAsync(ct);

            bool isOperating = exceptions.Count > 0
                ? exceptions[0]
                : regularService;

            if (!isOperating)
                throw new AppException(
                    "Trip does not operate on selected date.");

            // Pobieramy pozycje przystanków dla
            // wybranego wariantu trasy.
            var routeStops = await context.RouteStops
                .Where(x =>
                    x.RouteId == scheduledTrip.RouteId &&
                    (x.StopId == request.StartStopId.Value ||
                     x.StopId == request.EndStopId.Value))
                .Select(x => new
                {
                    x.StopId,
                    x.Sequence
                })
                .ToListAsync(ct);

            int? startSequence = routeStops
                .Where(x => x.StopId == request.StartStopId.Value)
                .Select(x => (int?)x.Sequence)
                .Min();

            int? endSequence = routeStops
                .Where(x => x.StopId == request.EndStopId.Value)
                .Select(x => (int?)x.Sequence)
                .Max();

            if (!startSequence.HasValue ||
                !endSequence.HasValue ||
                startSequence.Value >= endSequence.Value)
            {
                throw new AppException(
                    "Selected stops are not in the correct order.");
            }

            // Sprawdź również, czy kurs ma godziny
            // dla obu wskazanych pozycji przystankowych.
            var timedStops = await (
                from stopTime in context.ScheduledStopTimes
                join routeStop in context.RouteStops
                    on stopTime.RouteStopId equals routeStop.Id
                where stopTime.ScheduledTripId == scheduledTrip.Id
                    && routeStop.RouteId == scheduledTrip.RouteId
                    && (routeStop.StopId == request.StartStopId.Value ||
                        routeStop.StopId == request.EndStopId.Value)
                select new
                {
                    routeStop.StopId,
                    routeStop.Sequence,
                    stopTime.ArrivalTime,
                    stopTime.DepartureTime
                }
            ).ToListAsync(ct);

            bool validTimedPair = timedStops.Any(start =>
                start.StopId == request.StartStopId.Value &&
                timedStops.Any(end =>
                    end.StopId == request.EndStopId.Value &&
                    start.Sequence < end.Sequence &&
                    start.DepartureTime <= end.ArrivalTime));

            if (!validTimedPair)
                throw new AppException(
                    "No valid scheduled departure for selected stops.");

            // Historyczne kursy dopuszczamy wyłącznie
            // w skonfigurowanym trybie demonstracyjnym.
            if (configuration["Timetable:Mode"] != "Demo")
            {
                if (serviceDate < DateOnly.FromDateTime(DateTime.Today))
                    throw new AppException(
                        "Cannot purchase a ticket for a past trip.");
            }
        }

        var ticketType = await context.TicketTypes
            .FindAsync([request.TicketTypeId], ct)
            ?? throw new NotFoundException(
                "TicketType", request.TicketTypeId);

        if (!ticketType.IsActive)
            throw new AppException(
                "This ticket type is no longer available.");

        // Podstawowa walidacja strefy.
        // Pełne zasady taryfy dodamy osobno.
        var user = await context.Users.FindAsync([userId], ct)
            ?? throw new AppException("User not found.", 404);

        if (user.Balance < ticketType.Price)
            throw new AppException(
                "Insufficient funds. Please top up your wallet.");

        user.Balance -= ticketType.Price;

        var ticket = new Ticket
        {
            UserId = userId,
            TicketTypeId = ticketType.Id,
            Price = ticketType.Price,
            PurchasedAt = DateTime.UtcNow,
            IsValidated = false,

            ScheduledTripId = request.ScheduledTripId,
            ServiceDate = request.ServiceDate,
            StartStopId = request.StartStopId,
            EndStopId = request.EndStopId
        };

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync(ct);

        return ticket.Id;
    }
}
