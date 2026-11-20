using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeloCity.Api.Common.Pagination;
using VeloCity.Api.Features.Lines.Commands.AddStop;
using VeloCity.Api.Features.Lines.Commands.CreateLine;
using VeloCity.Api.Features.Lines.Commands.DeleteLine;
using VeloCity.Api.Features.Lines.Commands.RemoveStop;
using VeloCity.Api.Features.Lines.Commands.UpdateLine;
using VeloCity.Api.Features.Lines.Commands.UpdateSequence;
using VeloCity.Api.Features.Lines.DTOs;
using VeloCity.Api.Features.Lines.Queries.GetLineDetails;
using VeloCity.Api.Features.Lines.Queries.GetLines;
using VeloCity.Api.Features.Lines.Commands.ActivateLine;
using VeloCity.Api.Models;
using VeloCity.Api.Models.Enums;

namespace VeloCity.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class LinesController(IMediator mediator) 
    : ControllerBase
{
    // no auth, get all lines paginated
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<LineDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLines(
        CancellationToken ct,
        [FromQuery] string? searchTerm,
        [FromQuery] bool isDescending,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromQuery] bool includeInactive = false,
        [FromQuery] bool? isActive = null)
    {
        
        if (includeInactive &&
            !User.IsInRole(nameof(UserRole.Admin)))
        {
            return User.Identity?.IsAuthenticated == true
                ? Forbid()
                : Unauthorized();
        }

        var query = new GetLinesQuery(
            searchTerm,
            isDescending,
            pageNumber,
            pageSize,
            includeInactive,
            isActive);

        var response = await mediator.Send(query, ct);

        return Ok(response);

    }

    // no auth, get line details by id
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LineDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLineDetails(int id, CancellationToken ct)
    {
        var response = await mediator.Send(new GetLineDetailsQuery(id), ct);

        if (response is null) return NotFound();

        return Ok(response);
    }

    // admin only, create
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("create")]
    [ProducesResponseType(typeof(LineDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLine([FromBody] CreateLineCommand command, CancellationToken ct)
    {
        var lineDto = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetLineDetails), new { id = lineDto.Id }, lineDto);
    }

    // admin only , update
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLine(int id, [FromBody] UpdateLineBody body, CancellationToken ct)
    {
        var command = new UpdateLineCommand(id, body.Name);
        await mediator.Send(command, ct);
        return NoContent();
    }

    //admin only , delete
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteLine(int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteLineCommand(id), ct);
        return NoContent();
    }

    //admin only, add stop to line
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{id}/stops")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddStopToRoute(int id, [FromBody] AddStopBody body, CancellationToken ct)
    {
        var command = new AddStopCommand(id, body.StopId, body.Direction);
        await mediator.Send(command, ct);
        return NoContent();
    }

    //admin only , remove stop from line
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id}/stops/{stopId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveStopFromRoute(int routeStopId, CancellationToken ct)
    {
        var command = new RemoveStopCommand(routeStopId);
        await mediator.Send(command, ct);

        return NoContent();
    }

    //admin only, update stop sequence
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id}/sequence")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateRouteSequence(int id, [FromBody] UpdateSequenceBody body, CancellationToken ct)
    {
        var command = new UpdateSequenceCommand(id, body.Direction, body.NewStopIds);
        await mediator.Send(command, ct);

        return NoContent();
    }

    //ADMIN ONLY: activate line
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id:int}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActivateLine(
        int id,
        CancellationToken ct)
    {
        await mediator.Send(new ActivateLineCommand(id), ct);

        return NoContent();
    }

}
