namespace VeloCity.RouteImporter.Models;

public record MpkRouteVariant(
    int Id,
    string Name,
    int Direction,
    int RunsCount
);