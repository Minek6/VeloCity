using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Features.Dashboard.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Dashboard.Queries.GetTicketTypeSalesRanking;

public sealed class GetTicketTypeSalesRankingHandler(
    ApplicationDbContext context)
    : IRequestHandler<
        GetTicketTypeSalesRankingQuery,
        List<TicketTypeSalesDto>>
{
    public async Task<List<TicketTypeSalesDto>> Handle(
        GetTicketTypeSalesRankingQuery request,
        CancellationToken ct)
    {
        if (request.From > request.To ||
            request.To == DateOnly.MaxValue)
        {
            throw new AppException(
                "Invalid date range.", 400);
        }

        if (request.To.DayNumber - request.From.DayNumber > 365)
        {
            throw new AppException(
                "Maximum report range is 366 days.", 400);
        }

        if (request.TicketTypeId is <= 0)
        {
            throw new AppException(
                "Invalid ticket type ID.", 400);
        }

        var warsawZone =
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(
            request.From.ToDateTime(TimeOnly.MinValue),
            warsawZone);

        var endUtc = TimeZoneInfo.ConvertTimeToUtc(
            request.To.AddDays(1).ToDateTime(TimeOnly.MinValue),
            warsawZone);

        var query = context.Tickets
            .AsNoTracking()
            .Where(t =>
                t.PurchasedAt >= startUtc &&
                t.PurchasedAt < endUtc);

        if (request.TicketTypeId.HasValue)
        {
            query = query.Where(t =>
                t.TicketTypeId == request.TicketTypeId.Value);
        }

        if (request.IsValidated.HasValue)
        {
            query = query.Where(t =>
                t.IsValidated == request.IsValidated.Value);
        }

        var grouped = await query
            .GroupBy(t => new
            {
                t.TicketTypeId,
                t.TicketType.Name
            })
            .Select(g => new
            {
                TicketTypeId = g.Key.TicketTypeId,
                TicketTypeName = g.Key.Name,
                TicketsSold = g.Count(),
                TotalAmount = g.Sum(t => t.Price)
            })
            .OrderByDescending(g => g.TicketsSold)
            .ThenBy(g => g.TicketTypeId)
            .ToListAsync(ct);

        var totalTickets = grouped.Sum(x => x.TicketsSold);

        return grouped.Select(x => new TicketTypeSalesDto(
            x.TicketTypeId,
            x.TicketTypeName,
            x.TicketsSold,
            x.TotalAmount,
            totalTickets > 0
                ? Math.Round(
                    (decimal)x.TicketsSold / totalTickets * 100,
                    2)
                : 0m
        )).ToList();
    }
}
