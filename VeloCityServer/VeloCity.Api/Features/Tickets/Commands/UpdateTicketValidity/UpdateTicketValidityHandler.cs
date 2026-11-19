using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Common.Interfaces;
using VeloCity.Api.Models;
using VeloCity.Api.Models.Data;
using VeloCity.Api.Features.Tickets.DTOs;

namespace VeloCity.Api.Features.Tickets.Commands.UpdateTicketValidity;

public sealed class UpdateTicketValidityHandler(
    ApplicationDbContext context,
    IUserContext userContext)
    : IRequestHandler<UpdateTicketValidityCommand>
{
    public async Task Handle(
        UpdateTicketValidityCommand request,
        CancellationToken ct)
    {
        var adminId = userContext.Id
            ?? throw new AppException("Unauthorized access.", 401);

        if (!userContext.IsAdmin)
            throw new AppException("Admin access required.", 403);

        if (request.ValidTo.Kind != DateTimeKind.Utc)
            throw new AppException(
                "ValidTo must be provided in UTC.", 400);

        var ticket = await context.Tickets
            .Include(t => t.TicketType)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
            ?? throw new NotFoundException(
                "Ticket", request.TicketId);

        if (!ticket.IsValidated || ticket.ValidFrom is null)
            throw new AppException(
                "Ticket has not been activated.", 400);

        if (ticket.ValidTo is null)
            throw new AppException(
                "Ticket has no time-based expiration.", 400);

        if (request.ValidTo <= ticket.ValidFrom.Value)
            throw new AppException(
                "Expiration must be later than activation.", 400);

        if (request.ValidTo == ticket.ValidTo.Value)
            throw new AppException(
                "Expiration date has not changed.", 400);

        var previousValidTo = ticket.ValidTo.Value;

        await using var transaction =
            await context.Database.BeginTransactionAsync(ct);

        // Chronimy przed nadpisaniem równoczesnej korekty.
        var updated = await context.Tickets
            .Where(t =>
                t.Id == ticket.Id &&
                t.ValidTo == previousValidTo &&
                t.IsValidated)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    t => t.ValidTo, request.ValidTo),
                ct);

        if (updated != 1)
            throw new AppException(
                "Ticket was modified by another request.", 409);

        var adjustment = new TicketValidityAdjustment
        {
            TicketId = ticket.Id,
            AdminId = adminId,
            PreviousValidTo = previousValidTo,
            NewValidTo = request.ValidTo,
            Reason = request.Reason.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        context.TicketValidityAdjustments.Add(adjustment);

        await context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
