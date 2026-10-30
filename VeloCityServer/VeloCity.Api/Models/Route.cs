namespace VeloCity.Api.Models;

public class Route
{
    public int Id { get; set; }

    public int LineId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Direction { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual Line Line { get; set; } = null!;

    public virtual ICollection<RouteStop> RouteStops { get; set; }
        = new List<RouteStop>();
}
