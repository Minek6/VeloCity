using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Stops.Queries.GetMapStops;

public class GetMapStopsHandler(ApplicationDbContext context)
    : IRequestHandler<GetMapStopsQuery, List<MapStopDto>>
{
    public async Task<List<MapStopDto>> Handle(
        GetMapStopsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Stops
            .AsNoTracking()
            .Where(s =>
                s.IsActive &&
                s.Latitude >= -90 &&
                s.Latitude <= 90 &&
                s.Longitude >= -180 &&
                s.Longitude <= 180 &&
                (s.Latitude != 0 || s.Longitude != 0))
            .OrderBy(s => s.Name)
            .Select(s => new MapStopDto(
                s.Id,
                s.Name,
                s.Latitude,
                s.Longitude
            ))
            .ToListAsync(cancellationToken);
    }
}
