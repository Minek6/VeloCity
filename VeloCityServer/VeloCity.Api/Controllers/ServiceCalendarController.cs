using MediatR;
using Microsoft.AspNetCore.Mvc;
using VeloCity.Api.Features.ServiceCalendars.Commands.CreateServiceCalendar;

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
}
