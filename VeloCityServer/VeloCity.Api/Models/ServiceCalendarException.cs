namespace VeloCity.Api.Models;

public class ServiceCalendarException
{
    public int Id { get; set; }

    public int ServiceCalendarId { get; set; }

    public virtual ServiceCalendar ServiceCalendar { get; set; } = null!;

    public DateOnly Date { get; set; }

    public bool IsAdded { get; set; }
}
