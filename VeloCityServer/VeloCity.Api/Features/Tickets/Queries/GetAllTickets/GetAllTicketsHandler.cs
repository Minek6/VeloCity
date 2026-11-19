using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Pagination;
using VeloCity.Api.Features.Tickets.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Tickets.Queries.GetAllTickets;

public class GetAllTicketsHandler(ApplicationDbContext context)
    : IRequestHandler<GetAllTicketsQuery, PaginatedList<AdminTicketDto>>
{
    public async Task<PaginatedList<AdminTicketDto>> Handle(GetAllTicketsQuery request, CancellationToken ct)
    {
        var query = context.Tickets.AsNoTracking().AsQueryable();


        if (request.UserId.HasValue)
        {
            query = query.Where(t => t.UserId == request.UserId.Value);
        }

        if (request.VehicleId.HasValue)
        {
            query = query.Where(t => t.VehicleId == request.VehicleId.Value);
        }

        if (request.IsValidated.HasValue)
        {
            query = query.Where(t => t.IsValidated == request.IsValidated.Value);
        }

        var descending = request.IsDescending;

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "id" => descending
                ? query.OrderByDescending(t => t.Id)
                : query.OrderBy(t => t.Id),

            "price" => descending
                ? query.OrderByDescending(t => t.Price)
                    .ThenByDescending(t => t.Id)
                : query.OrderBy(t => t.Price)
                    .ThenBy(t => t.Id),

            "date" => descending
                ? query.OrderByDescending(t => t.PurchasedAt)
                    .ThenByDescending(t => t.Id)
                : query.OrderBy(t => t.PurchasedAt)
                    .ThenBy(t => t.Id),

            _ => query.OrderByDescending(t => t.PurchasedAt)
                .ThenByDescending(t => t.Id)
        };

        var projectedQuery = query.Select(t => new AdminTicketDto(
            t.Id,
            t.UserId,
            t.User.Name,
            t.User.Surname,
            t.TicketType.Name,
            t.Price,
            t.PurchasedAt,
            t.ValidFrom,
            t.ValidTo,
            t.VehicleId,
            t.IsValidated));

        return await PaginatedList<AdminTicketDto>.CreateAsync(projectedQuery, request.PageNumber, request.PageSize, ct);
    }
}
