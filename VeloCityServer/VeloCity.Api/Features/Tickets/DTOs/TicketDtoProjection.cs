
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Models;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Tickets.DTOs;

public static class TicketDtoProjection
{
    public static IQueryable<TicketDto> ToTicketDto(
        this IQueryable<Ticket> tickets,
        ApplicationDbContext context)
    {
        return tickets.Select(t => new TicketDto(
            t.Id,
            t.TicketType.Name,
            t.Price,
            t.PurchasedAt,
            t.ValidFrom,
            t.ValidTo,
            t.VehicleId,
            t.IsValidated,

            // Powiązanie z kursem
            t.ScheduledTripId,

            context.ScheduledTrips
                .Where(st => st.Id == t.ScheduledTripId)
                .Select(st => st.ExternalRunId)
                .FirstOrDefault(),

            t.ServiceDate,

            // Przystanek początkowy
            t.StartStopId,

            context.Stops
                .Where(s => s.Id == t.StartStopId)
                .Select(s => s.Name)
                .FirstOrDefault(),

            // Przystanek docelowy
            t.EndStopId,

            context.Stops
                .Where(s => s.Id == t.EndStopId)
                .Select(s => s.Name)
                .FirstOrDefault(),

            // Tymczasowe oznaczenie linii 6.
            // Przy imporcie kolejnych linii zastąpimy
            // to odczytem numeru linii z modelu Line.
            context.ScheduledTrips
                .Where(st =>
                    st.Id == t.ScheduledTripId &&
                    st.Route.LineId == 8)
                .Select(st => "6")
                .FirstOrDefault(),

            // Godzina odjazdu
            (
                from sst in context.ScheduledStopTimes
                join rs in context.RouteStops
                    on sst.RouteStopId equals rs.Id
                where sst.ScheduledTripId == t.ScheduledTripId
                      && rs.StopId == t.StartStopId
                orderby rs.Sequence
                select (TimeSpan?)sst.DepartureTime
            ).FirstOrDefault(),

            // Godzina przyjazdu
            (
                from sst in context.ScheduledStopTimes
                join rs in context.RouteStops
                    on sst.RouteStopId equals rs.Id
                where sst.ScheduledTripId == t.ScheduledTripId
                      && rs.StopId == t.EndStopId
                orderby rs.Sequence
                select (TimeSpan?)sst.ArrivalTime
            ).FirstOrDefault()
        ));
    }
}
