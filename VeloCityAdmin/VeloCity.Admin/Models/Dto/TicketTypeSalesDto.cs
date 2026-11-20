namespace VeloCity.Admin.Models.Dto;

public sealed class TicketTypeSalesDto
{
    public int TicketTypeId { get; set; }
    public string TicketTypeName { get; set; } = string.Empty;
    public int TicketsSold { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
}
