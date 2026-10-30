using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Features.ScheduledTrips.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.ScheduledTrips.Queries.GetScheduledTrips;

public class GetScheduledTripsHandler(ApplicationDbContext context)
    : IRequestHandler<GetScheduledTripsQuery, List<ScheduledTripListItemDto>>
{
    public async Task<List<ScheduledTripListItemDto>> Handle(
        GetScheduledTripsQuery request,
        CancellationToken ct)
    {
        var dayOfWeek = request.Date.DayOfWeek;

        var query = context.ScheduledTrips
            .AsNoTracking()
            .Where(st =>
                st.Route.IsActive &&
                st.Route.Line.IsActive &&
                st.ServiceCalendar.ValidFrom <= request.Date &&
                st.ServiceCalendar.ValidTo >= request.Date);

        query = dayOfWeek switch
        {
            DayOfWeek.Monday =>
                query.Where(st => st.ServiceCalendar.Monday),

            DayOfWeek.Tuesday =>
                query.Where(st => st.ServiceCalendar.Tuesday),

            DayOfWeek.Wednesday =>
                query.Where(st => st.ServiceCalendar.Wednesday),

            DayOfWeek.Thursday =>
                query.Where(st => st.ServiceCalendar.Thursday),

            DayOfWeek.Friday =>
                query.Where(st => st.ServiceCalendar.Friday),

            DayOfWeek.Saturday =>
                query.Where(st => st.ServiceCalendar.Saturday),

            DayOfWeek.Sunday =>
                query.Where(st => st.ServiceCalendar.Sunday),

            _ => query
        };

        if (request.LineId.HasValue)
        {
            query = query.Where(st =>
                st.Route.LineId == request.LineId.Value);
        }

        var trips = await query
            .Where(st => st.StopTimes.Count >= 2)
            .Select(st => new ScheduledTripListItemDto(
                st.Id,
                st.Route.LineId,
                st.Route.Line.Name,
                st.RouteId,
                st.Route.Name,
                st.Route.Direction,

                st.StopTimes
                    .OrderBy(sst => sst.RouteStop.Sequence)
                    .Select(sst => sst.DepartureTime)
                    .First(),

                st.StopTimes
                    .OrderByDescending(sst => sst.RouteStop.Sequence)
                    .Select(sst => sst.ArrivalTime)
                    .First(),

                st.StopTimes
                    .OrderBy(sst => sst.RouteStop.Sequence)
                    .Select(sst => sst.RouteStop.Stop.Name)
                    .First(),

                st.StopTimes
                    .OrderByDescending(sst => sst.RouteStop.Sequence)
                    .Select(sst => sst.RouteStop.Stop.Name)
                    .First()
            ))
            .ToListAsync(ct);

            return trips
                .OrderBy(st => st.DepartureTime)
                .ToList();
                }
}
