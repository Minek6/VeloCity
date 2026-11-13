namespace VeloCity.RouteImporter.Models;

public class RouteStopsImport
{
    public int ExternalVariantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Direction { get; set; }

    public List<RouteStopImportItem> Stops { get; set; } = new();
}

public class RouteStopImportItem
{
    public int Sequence { get; set; }

    public string MpkNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int? VeloCityStopId { get; set; }
}