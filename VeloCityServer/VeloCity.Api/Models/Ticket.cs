namespace VeloCity.Api.Models;

public class Ticket
{
    public int Id { get; set; }
    public int TicketTypeId { get; set; }
    public virtual TicketType TicketType { get; set; } = null!;
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsValidated { get; set; } = false;
    public decimal Price { get; set; }
    public int UserId { get; set; }
    public int? StartStopId { get; set; }
    public int? EndStopId { get; set; }
    public int? ScheduledTripId { get; set; }
    public DateOnly? ServiceDate { get; set; }
    public virtual User User { get; set; } = null!;
    public int? VehicleId { get; set; }
    public virtual Vehicle Vehicle { get; set; } = null!;
}
