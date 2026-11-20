using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Features.Dashboard.DTOs;
using VeloCity.Api.Models.Data;
using VeloCity.Api.Models.Enums;

namespace VeloCity.Api.Features.Dashboard.Queries.GetDashboardStats;

public sealed class GetDashboardStatsHandler(
    ApplicationDbContext context)
    : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    public async Task<DashboardStatsDto> Handle(
        GetDashboardStatsQuery request,
        CancellationToken ct)
    {
        var warsawZone = TimeZoneInfo.FindSystemTimeZoneById(
            "Europe/Warsaw");

        var now = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow, warsawZone);

        var today = now.Date;

        var dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(
            today, warsawZone);

        var dayEndUtc = TimeZoneInfo.ConvertTimeToUtc(
            today.AddDays(1), warsawZone);

        var activePassengers = await context.Users
            .AsNoTracking()
            .CountAsync(u =>
                u.IsActive &&
                u.Role == UserRole.Passenger, ct);

        var soldTickets = await context.Tickets
            .AsNoTracking()
            .CountAsync(ct);

        var validatedTickets = await context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.IsValidated, ct);

        var activeVehicles = await context.Vehicles
            .AsNoTracking()
            .CountAsync(v => v.IsActive, ct);

        var ticketsToday = await context.Tickets
            .AsNoTracking()
            .CountAsync(t =>
                t.PurchasedAt >= dayStartUtc &&
                t.PurchasedAt < dayEndUtc, ct);

        var totalTicketSales = await context.Tickets
            .AsNoTracking()
            .SumAsync(t => (decimal?)t.Price, ct) ?? 0m;

        return new DashboardStatsDto(
            activePassengers,
            soldTickets,
            validatedTickets,
            activeVehicles,
            ticketsToday,
            totalTicketSales);
    }
}
