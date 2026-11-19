namespace VeloCity.Api.Models;

public class TicketValidityAdjustment
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public int AdminId { get; set; }
    public User Admin { get; set; } = null!;

    public DateTime PreviousValidTo { get; set; }
    public DateTime NewValidTo { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
