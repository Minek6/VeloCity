using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Models;
using VeloCity.Api.Models.Data;
using VeloCity.Api.Common.Exceptions;

namespace VeloCity.Api.Features.Routes.Commands.AddRouteStop;

public class AddRouteStopHandler(ApplicationDbContext context)
    : IRequestHandler<AddRouteStopCommand, int>
{
    public async Task<int> Handle(
        AddRouteStopCommand request,
        CancellationToken ct)
    {
        // Sprawdzenie wariantu trasy
        var route = await context.Routes
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.Id == request.RouteId &&
                r.IsActive &&
                r.Line.IsActive, ct);

        if (route is null)
            throw new NotFoundException("Route", request.RouteId);

        // Sprawdzenie przystanku
        var stop = await context.Stops
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == request.StopId && s.IsActive, ct);

        if (stop is null)
            throw new NotFoundException("Stop", request.StopId);

        // Sprawdzenie zajętej pozycji
        var sequenceExists = await context.RouteStops
            .AnyAsync(rs =>
                rs.RouteId == request.RouteId &&
                rs.Sequence == request.Sequence, ct);

        if (sequenceExists)
            throw new ConflictException(
                "This sequence is already used in the route.");

        // Sprawdzenie duplikatu przystanku
        var stopExists = await context.RouteStops
            .AnyAsync(rs =>
                rs.RouteId == request.RouteId &&
                rs.StopId == request.StopId, ct);

        if (stopExists)
            throw new ConflictException(
                "This stop is already assigned to the route.");

        var routeStop = new RouteStop
        {
            RouteId = route.Id,
            LineId = route.LineId,
            Direction = route.Direction,
            StopId = request.StopId,
            Sequence = request.Sequence
        };

        context.RouteStops.Add(routeStop);
        await context.SaveChangesAsync(ct);

        return routeStop.Id;
    }
}
