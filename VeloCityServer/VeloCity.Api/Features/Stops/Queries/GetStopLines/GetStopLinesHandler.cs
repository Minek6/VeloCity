
using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Stops.Queries.GetStopLines;

public class GetStopLinesHandler(ApplicationDbContext context)
    : IRequestHandler<GetStopLinesQuery, List<StopLineDto>>
{
    public async Task<List<StopLineDto>> Handle(
        GetStopLinesQuery request,
        CancellationToken ct)
    {
        // Sprawdzamy istnienie przystanku.
        var stopExists = await context.Stops
            .AsNoTracking()
            .AnyAsync(s => s.Id == request.StopId, ct);

        if (!stopExists)
        {
            throw new NotFoundException("Stop", request.StopId);
        }

        // Pobieramy dane z bazy.
        var routeStops = await context.RouteStops
            .AsNoTracking()
            .Where(rs =>
                rs.StopId == request.StopId &&
                rs.Line.IsActive &&
                rs.Stop.IsActive &&
                (rs.RouteId == null || rs.Route!.IsActive))
            .Select(rs => new
            {
                rs.LineId,
                LineName = rs.Line.Name,
                rs.Direction,
                rs.RouteId,
                RouteName = rs.Route != null
                    ? rs.Route.Name
                    : null
            })
            .ToListAsync(ct);

        // Usuwamy duplikaty i sortujemy w pamięci.
        var lines = routeStops
            .DistinctBy(rs => new
            {
                rs.LineId,
                rs.Direction,
                rs.RouteId
            })
            .OrderBy(rs => rs.LineName)
            .ThenBy(rs => rs.Direction)
            .Select(rs => new StopLineDto(
                rs.LineId,
                rs.LineName,
                rs.Direction,
                rs.RouteId,
                rs.RouteName
            ))
            .ToList();

        return lines;
    }
}
