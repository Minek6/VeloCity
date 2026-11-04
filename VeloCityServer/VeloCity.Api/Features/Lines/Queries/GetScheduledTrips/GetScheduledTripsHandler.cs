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
                st.Route.Line.IsActive);

        query = query.Where(st =>
            // Wyjątek dodający kursowanie ma pierwszeństwo.
            st.ServiceCalendar.Exceptions.Any(e =>
                e.Date == request.Date && e.IsAdded)

            ||

            (
                // Standardowy zakres ważności kalendarza.
                st.ServiceCalendar.ValidFrom <= request.Date &&
                st.ServiceCalendar.ValidTo >= request.Date &&

                // Standardowe dni tygodnia.
                (
                    (dayOfWeek == DayOfWeek.Monday && st.ServiceCalendar.Monday) ||
                    (dayOfWeek == DayOfWeek.Tuesday && st.ServiceCalendar.Tuesday) ||
                    (dayOfWeek == DayOfWeek.Wednesday && st.ServiceCalendar.Wednesday) ||
                    (dayOfWeek == DayOfWeek.Thursday && st.ServiceCalendar.Thursday) ||
                    (dayOfWeek == DayOfWeek.Friday && st.ServiceCalendar.Friday) ||
                    (dayOfWeek == DayOfWeek.Saturday && st.ServiceCalendar.Saturday) ||
                    (dayOfWeek == DayOfWeek.Sunday && st.ServiceCalendar.Sunday)
                )

                // Wyjątek wyłączający kursowanie.
                &&
                !st.ServiceCalendar.Exceptions.Any(e =>
                    e.Date == request.Date && !e.IsAdded)
            )
        );


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
