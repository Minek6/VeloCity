using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Models.Data;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Models;

namespace VeloCity.Api.Features.ScheduledTrips.Commands.CreateScheduledTrip;

public class CreateScheduledTripHandler(ApplicationDbContext context)
    : IRequestHandler<CreateScheduledTripCommand, int>
{
    public async Task<int> Handle(
        CreateScheduledTripCommand request,
        CancellationToken cancellationToken)
    {
        var route = await context.Routes
            .AsNoTracking()
            .Include(r => r.RouteStops)
            .FirstOrDefaultAsync(
                r => r.Id == request.RouteId
                     && r.IsActive
                     && r.Line.IsActive,
                cancellationToken);

        if (route is null)
            throw new NotFoundException("Route", request.RouteId);

        var calendarExists = await context.ServiceCalendars
            .AnyAsync(c => c.Id == request.ServiceCalendarId,
                cancellationToken);

        if (!calendarExists)
            throw new NotFoundException(
                "ServiceCalendar",
                request.ServiceCalendarId);

        var routeStops = route.RouteStops
            .OrderBy(rs => rs.Sequence)
            .ToList();

        if (routeStops.Count == 0)
            throw new ConflictException(
                "The route does not contain any stops.");

        if (request.StopTimes.Count != routeStops.Count)
            throw new ConflictException(
                "Stop times must contain all route stops.");

        for (var i = 0; i < routeStops.Count; i++)
        {
            var stopTime = request.StopTimes[i];

            if (stopTime.RouteStopId != routeStops[i].Id)
                throw new ConflictException(
                    "Stop times must follow the route stop sequence.");

            if (i > 0 &&
                stopTime.ArrivalTime < request.StopTimes[i - 1].DepartureTime)
                throw new ConflictException(
                    "Arrival time cannot be earlier than the previous departure.");
        }

        var scheduledTrip = new ScheduledTrip
        {
            RouteId = request.RouteId,
            ServiceCalendarId = request.ServiceCalendarId,

            StopTimes = request.StopTimes
                .Select(st => new ScheduledStopTime
                {
                    RouteStopId = st.RouteStopId,
                    ArrivalTime = st.ArrivalTime,
                    DepartureTime = st.DepartureTime
                })
                .ToList()
        };

        context.ScheduledTrips.Add(scheduledTrip);

        await context.SaveChangesAsync(cancellationToken);

        return scheduledTrip.Id;
    }
}
