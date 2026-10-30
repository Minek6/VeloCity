namespace VeloCity.Api.Features.ScheduledTrips.DTOs;

public record ScheduledStopTimeDto(
    int StopId,
    string StopName,
    int Sequence,
    TimeSpan ArrivalTime,
    TimeSpan DepartureTime
);

public record ScheduledTripDetailsDto(
    int Id,
    int LineId,
    string LineName,
    int RouteId,
    string RouteName,
    int Direction,
    string CalendarName,
    DateOnly ValidFrom,
    DateOnly ValidTo,
    bool Monday,
    bool Tuesday,
    bool Wednesday,
    bool Thursday,
    bool Friday,
    bool Saturday,
    bool Sunday,
    List<ScheduledStopTimeDto> Stops
);
