namespace VeloCity.Api.Models;

public class ScheduledTrip
{
    public int Id { get; set; }

    // Wariant trasy
    public int RouteId { get; set; }
    public virtual Route Route { get; set; } = null!;

    // Kalendarz obowiązywania
    public int ServiceCalendarId { get; set; }
    public virtual ServiceCalendar ServiceCalendar { get; set; } = null!;

    // Planowany kurs może mieć wiele realizacji
    public virtual ICollection<Trip> Trips { get; set; }
        = new List<Trip>();
    
    // Planowane godziny odjazdów na przystankach
    public virtual ICollection<ScheduledStopTime> StopTimes { get; set; }
    = new List<ScheduledStopTime>();
}
