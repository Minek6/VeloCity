using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Tickets.Queries.GetAdminTicketTypes;

public sealed class GetAdminTicketTypesHandler(
    ApplicationDbContext context)
    : IRequestHandler<
        GetAdminTicketTypesQuery,
        List<AdminTicketTypeDto>>
{
    public async Task<List<AdminTicketTypeDto>> Handle(
        GetAdminTicketTypesQuery request,
        CancellationToken ct)
    {
        return await context.TicketTypes
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new AdminTicketTypeDto(
                t.Id,
                t.Name,
                t.Price,
                t.DurationInMinutes,
                t.ZoneLimit,
                t.IsActive))
            .ToListAsync(ct);
    }
}
