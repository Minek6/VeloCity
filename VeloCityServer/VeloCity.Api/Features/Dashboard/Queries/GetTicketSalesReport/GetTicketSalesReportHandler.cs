using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Features.Dashboard.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Dashboard.Queries.GetTicketSalesReport;

public sealed class GetTicketSalesReportHandler(
    ApplicationDbContext context)
    : IRequestHandler<
        GetTicketSalesReportQuery,
        List<DailyTicketSalesDto>>
{
    public async Task<List<DailyTicketSalesDto>> Handle(
        GetTicketSalesReportQuery request,
        CancellationToken ct)
    {
        if (request.From > request.To)
            throw new AppException(
                "Start date cannot be after end date.", 400);

        if (request.To.DayNumber - request.From.DayNumber > 365)
            throw new AppException(
                "Maximum report range is 366 days.", 400);

        if (request.TicketTypeId is <= 0)
            throw new AppException(
                "Invalid ticket type ID.", 400);

        var warsawZone =
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(
            request.From.ToDateTime(TimeOnly.MinValue),
            warsawZone);

        var endUtc = TimeZoneInfo.ConvertTimeToUtc(
            request.To.AddDays(1).ToDateTime(TimeOnly.MinValue),
            warsawZone);

        var query = context.Tickets.AsNoTracking()
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

        var tickets = await query
            .Select(t => new
            {
                t.PurchasedAt,
                t.Price
            })
            .ToListAsync(ct);

        var grouped = tickets
            .GroupBy(t => DateOnly.FromDateTime(
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

        for (var date = request.From;
             date <= request.To;
             date = date.AddDays(1))
        {
            if (grouped.TryGetValue(date, out var sales))
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
