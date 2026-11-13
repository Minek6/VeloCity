
using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Common.Interfaces;
using VeloCity.Api.Features.Tickets.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Tickets.Queries.GetMyActiveTickets;

public class GetMyActiveTicketsHandler(
    ApplicationDbContext context,
    IUserContext userContext)
    : IRequestHandler<GetMyActiveTicketsQuery, List<TicketDto>>
{
    public async Task<List<TicketDto>> Handle(
        GetMyActiveTicketsQuery request,
        CancellationToken ct)
    {
        var userId = userContext.Id
            ?? throw new AppException("Unauthorized access.", 401);

        var now = DateTime.UtcNow;

        return await context.Tickets
            .AsNoTracking()
            .Where(t =>
                t.UserId == userId &&
                t.IsValidated &&
                (t.ValidTo == null || t.ValidTo > now))
            .OrderBy(t => t.ValidTo)
            .ToTicketDto(context)
            .ToListAsync(ct);
    }
}
