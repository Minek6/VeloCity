namespace VeloCity.Api.Models;

public class ServiceCalendar
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly ValidFrom { get; set; }
    public DateOnly ValidTo { get; set; }

    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }
    public bool Saturday { get; set; }
    public bool Sunday { get; set; }

    public virtual ICollection<ScheduledTrip> ScheduledTrips
    { get; set; } = new List<ScheduledTrip>();
}
