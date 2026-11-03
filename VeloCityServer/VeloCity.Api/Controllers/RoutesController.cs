using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VeloCity.Api.Features.Routes.Commands.CreateRoute;
using VeloCity.Api.Features.Routes.Commands.AddRouteStop;
using VeloCity.Api.Models.Enums;


namespace VeloCity.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class RoutesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    //[Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRoute(
        [FromBody] CreateRouteCommand command,
        CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);

        return StatusCode(StatusCodes.Status201Created, id);
    }

    
    [HttpPost("{id:int}/stops")]
    //[Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddRouteStop(
        int id,
        [FromBody] AddRouteStopRequest request,
        CancellationToken ct)
    {
        var routeStopId = await mediator.Send(
            new AddRouteStopCommand(
                id,
                request.StopId,
                request.Sequence),
            ct);

        return StatusCode(
            StatusCodes.Status201Created,
            routeStopId);
    }

    public record AddRouteStopRequest(
        int StopId,
        int Sequence
    );

}
