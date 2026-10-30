namespace VeloCity.Api.Features.ScheduledTrips.DTOs;

public record ScheduledTripListItemDto(
    int Id,
    int LineId,
    string LineName,
    int RouteId,
    string RouteName,
    int Direction,
    TimeSpan DepartureTime,
    TimeSpan ArrivalTime,
    string FirstStopName,
    string LastStopName
);
