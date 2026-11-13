
using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Common.Interfaces;
using VeloCity.Api.Common.Pagination;
using VeloCity.Api.Features.Tickets.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Tickets.Queries.GetMyTicketsHistory;

public class GetMyTicketsHistoryHandler(
    ApplicationDbContext context,
    IUserContext userContext)
    : IRequestHandler<GetMyTicketsHistoryQuery, PaginatedList<TicketDto>>
{
    public async Task<PaginatedList<TicketDto>> Handle(
        GetMyTicketsHistoryQuery request,
        CancellationToken ct)
    {
        var userId = userContext.Id
            ?? throw new AppException("Unauthorized access.", 401);

        var query = context.Tickets
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.PurchasedAt)
            .ThenByDescending(t => t.Id)
            .ToTicketDto(context);

        return await PaginatedList<TicketDto>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize,
            ct);
    }
}
