using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Features.ScheduledTrips.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.ScheduledTrips.Queries.GetScheduledTripDetails;

public class GetScheduledTripDetailsHandler(ApplicationDbContext context)
    : IRequestHandler<GetScheduledTripDetailsQuery, ScheduledTripDetailsDto>
{
    public async Task<ScheduledTripDetailsDto> Handle(
        GetScheduledTripDetailsQuery request,
        CancellationToken ct)
    {
        return await context.ScheduledTrips
            .AsNoTracking()
            .Where(st => st.Id == request.Id)
            .Select(st => new ScheduledTripDetailsDto(
                st.Id,
                st.Route.LineId,
                st.Route.Line.Name,
                st.RouteId,
                st.Route.Name,
                st.Route.Direction,
                st.ServiceCalendar.Name,
                st.ServiceCalendar.ValidFrom,
                st.ServiceCalendar.ValidTo,
                st.ServiceCalendar.Monday,
                st.ServiceCalendar.Tuesday,
                st.ServiceCalendar.Wednesday,
                st.ServiceCalendar.Thursday,
                st.ServiceCalendar.Friday,
                st.ServiceCalendar.Saturday,
                st.ServiceCalendar.Sunday,
                st.StopTimes
                    .OrderBy(sst => sst.RouteStop.Sequence)
                    .Select(sst => new ScheduledStopTimeDto(
                        sst.RouteStop.StopId,
                        sst.RouteStop.Stop.Name,
                        sst.RouteStop.Sequence,
                        sst.ArrivalTime,
                        sst.DepartureTime
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("ScheduledTrip", request.Id);
    }
}
