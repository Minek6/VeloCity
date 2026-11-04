using MediatR;
using Microsoft.AspNetCore.Mvc;
using VeloCity.Api.Features.ServiceCalendars.Commands.CreateServiceCalendar;
using Microsoft.AspNetCore.Authorization;
using VeloCity.Api.Features.ServiceCalendars.Commands.CreateServiceCalendarException;
using VeloCity.Api.Models;

namespace VeloCity.Api.Controllers;

[ApiController]
[Route("service-calendars")]
public class ServiceCalendarsController(IMediator mediator)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateServiceCalendar(
        [FromBody] CreateServiceCalendarCommand command,
        CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);

        return StatusCode(StatusCodes.Status201Created, id);
    }

    //[Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{id:int}/exceptions")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateException(
        int id,
        [FromBody] CreateServiceCalendarExceptionRequest request,
        CancellationToken cancellationToken)
    {
        var exceptionId = await mediator.Send(
            new CreateServiceCalendarExceptionCommand(
                id,
                request.Date,
                request.IsAdded),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            exceptionId);
    }
}

public record CreateServiceCalendarExceptionRequest(
    DateOnly Date,
    bool IsAdded
);
