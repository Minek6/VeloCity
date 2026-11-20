namespace VeloCity.Api.Features.Dashboard.DTOs;

public record DashboardStatsDto(
    int ActivePassengers,
    int SoldTickets,
    int ValidatedTickets,
    int ActiveVehicles,
    int TicketsToday,
    decimal TotalTicketSales);
