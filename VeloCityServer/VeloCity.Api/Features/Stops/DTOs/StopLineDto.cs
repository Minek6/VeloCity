namespace VeloCity.Api.Features.Stops.Queries.GetStopLines;

public record StopLineDto(
    int LineId,
    string LineName,
    int Direction,
    int? RouteId,
    string? RouteName
);