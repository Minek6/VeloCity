using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeloCity.Api.Common.Pagination;
using VeloCity.Api.Features.Stops;
using VeloCity.Api.Features.Stops.Commands.CreateStop;
using VeloCity.Api.Features.Stops.Commands.DeleteStop;
using VeloCity.Api.Features.Stops.Commands.UpdateStop;
using VeloCity.Api.Features.Stops.DTOs;
using VeloCity.Api.Features.Stops.Queries.GetStop;
using VeloCity.Api.Features.Stops.Queries.GetStops;
using VeloCity.Api.Features.Stops.Queries.GetMapStops;
using VeloCity.Api.Features.Stops.Queries.GetStopLines;
using VeloCity.Api.Models.Enums;

namespace VeloCity.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class StopsController(IMediator mediator) : ControllerBase
{
    // no auth get stops list paginated
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<StopDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStops([FromQuery] GetStopsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result);
    }

    // no auth, get single stop
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(StopDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStop(int id)
    {
        var result = await mediator.Send(new GetStopQuery(id));
        return Ok(result);
    }

    // no auth get map stops
    [HttpGet("map")]
    [ProducesResponseType(
        typeof(List<MapStopDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMapStops(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetMapStopsQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}/lines")]
    public async Task<ActionResult<List<StopLineDto>>> GetStopLines(
        int id,
        CancellationToken ct)
    {
        var lines = await mediator.Send(
            new GetStopLinesQuery(id),
            ct
        );

        return Ok(lines);
    }

    // ADMIN create stop
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateStop([FromBody] CreateStopCommand command)
    {
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetStop), new { id }, id);
    }

    // ADMIN update stop
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateStop(int id, [FromBody] UpdateStopCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch.");
        await mediator.Send(command);
        return NoContent();
    }

    // ADMIN delete stop
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteStop(int id)
    {
        await mediator.Send(new DeleteStopCommand(id));
        return NoContent();
    }

}
