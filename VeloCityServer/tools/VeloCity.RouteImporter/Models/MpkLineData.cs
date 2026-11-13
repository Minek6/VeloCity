namespace VeloCity.RouteImporter.Models;

public class MpkLineData
{
    public string LineName { get; set; } = string.Empty;

    public int TimetableId { get; set; }

    public List<MpkStop> Stops { get; set; } = new();

    public List<MpkRouteVariant> Variants { get; set; } = new();
}