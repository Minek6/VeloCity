using MediatR;
using VeloCity.Api.Features.ScheduledTrips.DTOs;

namespace VeloCity.Api.Features.ScheduledTrips.Queries.GetScheduledTrips;

public record GetScheduledTripsQuery(
    DateOnly Date,
    int? LineId = null
) : IRequest<List<ScheduledTripListItemDto>>;
