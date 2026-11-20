using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Features.Dashboard.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Dashboard.Queries.GetTicketSalesChart;

public sealed class GetTicketSalesChartHandler(
    ApplicationDbContext context)
    : IRequestHandler<
        GetTicketSalesChartQuery,
        List<DailyTicketSalesDto>>
{
    public async Task<List<DailyTicketSalesDto>> Handle(
        GetTicketSalesChartQuery request,
        CancellationToken ct)
    {
        if (request.Days is not (7 or 30))
            throw new AppException(
                "Supported periods are 7 and 30 days.", 400);

        var warsawZone =
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

        var today = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow, warsawZone));

        var firstDay = today.AddDays(-(request.Days - 1));

        var startLocal = firstDay.ToDateTime(TimeOnly.MinValue);
        var endLocal = today.AddDays(1)
            .ToDateTime(TimeOnly.MinValue);

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(
            startLocal, warsawZone);

        var endUtc = TimeZoneInfo.ConvertTimeToUtc(
            endLocal, warsawZone);

        var tickets = await context.Tickets
            .AsNoTracking()
            .Where(t =>
                t.PurchasedAt >= startUtc &&
                t.PurchasedAt < endUtc)
            .Select(t => new
            {
                t.PurchasedAt,
                t.Price
            })
            .ToListAsync(ct);

        var salesByDay = tickets
            .GroupBy(t =>
                DateOnly.FromDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.SpecifyKind(
                            t.PurchasedAt,
                            DateTimeKind.Utc),
                        warsawZone)))
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Count = g.Count(),
                    Amount = g.Sum(t => t.Price)
                });

        var result = new List<DailyTicketSalesDto>();

        for (var i = 0; i < request.Days; i++)
        {
            var date = firstDay.AddDays(i);

            if (salesByDay.TryGetValue(date, out var sales))
            {
                result.Add(new DailyTicketSalesDto(
                    date,
                    sales.Count,
                    sales.Amount));
            }
            else
            {
                result.Add(new DailyTicketSalesDto(
                    date, 0, 0m));
            }
        }

        return result;
    }
}
