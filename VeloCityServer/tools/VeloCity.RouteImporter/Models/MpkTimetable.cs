
namespace VeloCity.RouteImporter.Models;

public class MpkTimetable
{
    public string LineNumber { get; set; } = string.Empty;

    public string StopCode { get; set; } = string.Empty;

    public string StopName { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;

    public List<string> RouteStops { get; set; } = new();

    public List<MpkDeparture> WeekdayCourses { get; set; } = new();

    public List<MpkDeparture> SaturdayCourses { get; set; } = new();

    public List<MpkDeparture> SundayCourses { get; set; } = new();

    public Dictionary<string, string> CourseLegend { get; set; } = new();
}
