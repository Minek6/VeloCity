namespace VeloCity.Admin.Models.Dto;

public sealed class DailyTicketSalesDto
{
    public DateOnly Date { get; set; }

    public int TicketsSold { get; set; }

    public decimal TotalAmount { get; set; }
}
