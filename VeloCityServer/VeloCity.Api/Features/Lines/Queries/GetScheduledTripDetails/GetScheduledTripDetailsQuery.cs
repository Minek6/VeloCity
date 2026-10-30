using MediatR;
using VeloCity.Api.Features.ScheduledTrips.DTOs;

namespace VeloCity.Api.Features.ScheduledTrips.Queries.GetScheduledTripDetails;

public record GetScheduledTripDetailsQuery(int Id)
    : IRequest<ScheduledTripDetailsDto>;
