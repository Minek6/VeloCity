using System.Text.Json.Serialization;

namespace VeloCity.RouteImporter.Models;

public class ApprovedStopMatches
{
    [JsonPropertyName("line")]
    public string Line { get; set; } = string.Empty;

    [JsonPropertyName("timetableId")]
    public int TimetableId { get; set; }

    [JsonPropertyName("matches")]
    public List<ApprovedStopMatch> Matches { get; set; } = new();
}

public class ApprovedStopMatch
{
    [JsonPropertyName("mpkNumber")]
    public string MpkNumber { get; set; } = string.Empty;

    [JsonPropertyName("veloCityStopId")]
    public int VeloCityStopId { get; set; }
}