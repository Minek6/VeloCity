using MediatR;

namespace VeloCity.Api.Features.ScheduledTrips.Commands.CreateScheduledTrip;

public record CreateScheduledTripCommand(
    int RouteId,
    int ServiceCalendarId,
    List<CreateScheduledStopTimeDto> StopTimes
) : IRequest<int>;

public record CreateScheduledStopTimeDto(
    int RouteStopId,
    TimeSpan ArrivalTime,
    TimeSpan DepartureTime
);
