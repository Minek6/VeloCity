using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Features.Tickets.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Tickets.Queries.GetTicketValidityAdjustments;

public sealed class GetTicketValidityAdjustmentsHandler(
    ApplicationDbContext context)
    : IRequestHandler<
        GetTicketValidityAdjustmentsQuery,
        List<TicketValidityAdjustmentDto>>
{
    public async Task<List<TicketValidityAdjustmentDto>> Handle(
        GetTicketValidityAdjustmentsQuery request,
        CancellationToken ct)
    {
        var ticketExists = await context.Tickets
            .AnyAsync(t => t.Id == request.TicketId, ct);

        if (!ticketExists)
        {
            throw new NotFoundException(
                "Ticket", request.TicketId);
        }

        return await context.TicketValidityAdjustments
            .AsNoTracking()
            .Where(x => x.TicketId == request.TicketId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new TicketValidityAdjustmentDto(
                x.Id,
                x.TicketId,
                x.AdminId,
                x.Admin.Name,
                x.Admin.Surname,
                x.PreviousValidTo,
                x.NewValidTo,
                x.Reason,
                x.CreatedAt))
            .ToListAsync(ct);
    }
}
