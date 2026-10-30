namespace VeloCity.Api.Models;

public class ScheduledStopTime
{
    public int Id { get; set; }

    // Planowany kurs
    public int ScheduledTripId { get; set; }
    public virtual ScheduledTrip ScheduledTrip { get; set; } = null!;

    // Konkretny przystanek na wariancie trasy
    public int RouteStopId { get; set; }
    public virtual RouteStop RouteStop { get; set; } = null!;

    // Planowane godziny
    public TimeSpan ArrivalTime { get; set; }
    public TimeSpan DepartureTime { get; set; }
}
