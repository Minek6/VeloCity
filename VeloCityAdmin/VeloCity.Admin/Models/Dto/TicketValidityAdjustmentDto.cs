namespace VeloCity.Admin.Models.Dto;

public sealed class TicketValidityAdjustmentDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int AdminId { get; set; }
    public string AdminName { get; set; } = "";
    public string AdminSurname { get; set; } = "";
    public DateTime PreviousValidTo { get; set; }
    public DateTime NewValidTo { get; set; }
    public string Reason { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
