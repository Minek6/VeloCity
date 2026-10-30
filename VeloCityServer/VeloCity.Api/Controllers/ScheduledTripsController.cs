using MediatR;
using Microsoft.AspNetCore.Mvc;
using VeloCity.Api.Features.ScheduledTrips.DTOs;
using VeloCity.Api.Features.ScheduledTrips.Queries.GetScheduledTripDetails;
using VeloCity.Api.Features.ScheduledTrips.Queries.GetScheduledTrips;

namespace VeloCity.Api.Controllers;

[ApiController]
[Route("scheduled-trips")]
public class ScheduledTripsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ScheduledTripDetailsDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScheduledTripDetails(
        int id,
        CancellationToken ct)
    {
        var response = await mediator.Send(
            new GetScheduledTripDetailsQuery(id), ct);

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(List<ScheduledTripListItemDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScheduledTrips(
        [FromQuery] DateOnly date,
        [FromQuery] int? lineId,
        CancellationToken ct)
    {
        var response = await mediator.Send(
            new GetScheduledTripsQuery(date, lineId),
            ct
        );

        return Ok(response);
    }

}


