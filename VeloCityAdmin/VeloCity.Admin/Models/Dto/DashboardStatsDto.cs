namespace VeloCity.Admin.Models.Dto;

public sealed class DashboardStatsDto
{
    public int ActivePassengers { get; set; }
    public int SoldTickets { get; set; }
    public int ValidatedTickets { get; set; }
    public int ActiveVehicles { get; set; }
    public int TicketsToday { get; set; }
    public decimal TotalTicketSales { get; set; }
}
