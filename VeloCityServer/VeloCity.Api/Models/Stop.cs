namespace VeloCity.Api.Models;

public class Stop
{
    public int Id { get; set; }
    public int ExternalId { get; set; }
    public long? OsmId { get; set; }
    public string? OsmType { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsActive { get; set; }
    public int ZoneId { get; set; } = 0;
    public virtual ICollection<RouteStop> RouteStops { get; set; } = new List<RouteStop>();
}
