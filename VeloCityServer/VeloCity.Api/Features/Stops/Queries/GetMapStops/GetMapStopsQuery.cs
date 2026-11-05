using MediatR;

namespace VeloCity.Api.Features.Stops.Queries.GetMapStops;

public record GetMapStopsQuery()
    : IRequest<List<MapStopDto>>;

public record MapStopDto(
    int Id,
    string Name,
    double Latitude,
    double Longitude
);
