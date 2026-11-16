using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeloCity.Api.Common.Pagination;
using VeloCity.Api.Features.Tickets.Commands.CreateTicketType;
using VeloCity.Api.Features.Tickets.Commands.DeleteTicketType;
using VeloCity.Api.Features.Tickets.Commands.PurchaseTicket;
using VeloCity.Api.Features.Tickets.Commands.UpdateTicketType;
using VeloCity.Api.Features.Tickets.Commands.ValidateTicket;
using VeloCity.Api.Features.Tickets.DTOs;
using VeloCity.Api.Features.Tickets.Queries.GetActiveTicketTypes;
using VeloCity.Api.Features.Tickets.Queries.GetAllTickets;
using VeloCity.Api.Features.Tickets.Queries.GetMyActiveTickets;
using VeloCity.Api.Features.Tickets.Queries.GetMyTicketsHistory;
using VeloCity.Api.Features.Tickets.Queries.GetTicketType;
using VeloCity.Api.Features.Tickets.Queries.VerifyTicket;
using VeloCity.Api.Common.Services.Qr;
using VeloCity.Api.Models.Enums;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Services;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Controllers;

[ApiController]
[Route("[controller]")]

public class TicketsController(
    IMediator mediator) : ControllerBase
{
    #region TicketTypes
    // get all tickets types
    [Authorize]
    [HttpGet("types")]
    [ProducesResponseType(typeof(List<TicketTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveTicketTypes()
    {
        var response = await mediator.Send(new GetActiveTicketTypesQuery());
        return Ok(response);
    }

    // get single tickets types
    [Authorize]
    [HttpGet("types/{id}")]
    [ProducesResponseType(typeof(TicketTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicketType(int id)
    {
        var response = await mediator.Send(new GetTicketTypeQuery(id));
        return Ok(response);
    }

    // ADMIN ONLY: delete ticket type
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("types/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteTicketType(int id)
    {
        await mediator.Send(new DeleteTicketTypeCommand(id));
        return NoContent();
    }

    // ADMIN ONLY: Create ticket type
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("types")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTicketType([FromBody] CreateTicketTypeCommand command)
    {
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetTicketType), new { id }, id);
    }

    // ADMIN ONLY: Update ticket type
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("types/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTicketType(int id, [FromBody] UpdateTicketTypeCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch.");

        await mediator.Send(command);
        return NoContent();
    }
    #endregion

    #region Tickets
    // Passenger buy ticket
    [Authorize]
    [HttpPost("purchase")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PurchaseTicket([FromBody] PurchaseTicketCommand command)
    {
        var ticketId = await mediator.Send(command);
        return Ok(ticketId);
    }

    // Passenger ticket validation
    [Authorize]
    [HttpPatch("{id}/validate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateTicket(int id, [FromBody] ValidateTicketCommand command)
    {
        if (id != command.TicketId) return BadRequest("ID mismatch.");
        await mediator.Send(command);
        return NoContent();
    }

    // get my active tickets
    [Authorize]
    [HttpGet("my/active")]
    [ProducesResponseType(typeof(List<TicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyActiveTickets()
    {
        var response = await mediator.Send(new GetMyActiveTicketsQuery());
        return Ok(response);
    }

    // My tickets - paginated
    [Authorize]
    [HttpGet("my/history")]
    [ProducesResponseType(typeof(PaginatedList<TicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTicketsHistory([FromQuery] GetMyTicketsHistoryQuery query)
    {
        var response = await mediator.Send(query);
        return Ok(response);
    }

    // For inspector ticket verify - in this system inspector == driver
    [Authorize(Roles = nameof(UserRole.Driver))]
    [HttpGet("{id}/verify")]
    [ProducesResponseType(typeof(TicketVerificationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyTicket(int id, [FromQuery] int vehicleId)
    {
        var query = new VerifyTicketQuery(id, vehicleId);
        var response = await mediator.Send(query);
        return Ok(response);
    }

    // ADMIN get all tickets Paginated
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<TicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTickets([FromQuery] GetAllTicketsQuery query)
    {
        var response = await mediator.Send(query);
        return Ok(response);
    }


    [Authorize]
    [HttpGet("{ticketId:int}/qr")]
    public async Task<IActionResult> GetTicketQr(
        int ticketId,
        [FromServices] ApplicationDbContext context,
        [FromServices] TicketQrService qrService,
        CancellationToken ct)
    {
        if (!int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var userId))
        {
            return Unauthorized();
        }

        var now = DateTime.UtcNow;

        var ticket = await context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.Id == ticketId &&
                t.UserId == userId, ct);

        if (ticket is null)
            return NotFound();

        if (!ticket.IsValidated ||
            ticket.ValidFrom is null ||
            ticket.ValidFrom > now ||
            ticket.ValidTo is null ||
            ticket.ValidTo <= now)
        {
            return BadRequest(new
            {
                message = "Ticket is not currently valid."
            });
        }

        return Ok(new
        {
            token = qrService.Generate(ticket.Id)
        });
    }



public record VerifyQrRequest(string Token);

    [Authorize(Roles = "Admin,Controller,Driver")]
    [HttpPost("verify-qr")]
    public async Task<IActionResult> VerifyQr(
        [FromBody] VerifyQrRequest request,
        [FromServices] ApplicationDbContext context,
        [FromServices] TicketQrService qrService,
        CancellationToken ct)
    {
        if (request is null ||
            !qrService.TryVerify(request.Token, out var ticketId))
        {
            return Ok(new
            {
                isValid = false,
                status = "InvalidQr"
            });
        }

        var ticket = await context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == ticketId, ct);

        if (ticket is null)
        {
            return Ok(new
            {
                isValid = false,
                status = "NotFound"
            });
        }

        var now = DateTime.UtcNow;

        if (!ticket.IsValidated ||
            ticket.ValidFrom is null)
        {
            return Ok(new
            {
                isValid = false,
                status = "NotActivated"
            });
        }

        if (ticket.ValidFrom > now ||
            ticket.ValidTo is null ||
            ticket.ValidTo <= now)
        {
            return Ok(new
            {
                isValid = false,
                status = "Expired"
            });
        }

        return Ok(new
        {
            isValid = true,
            status = "Valid",
            ticketId = ticket.Id,
            validFrom = ticket.ValidFrom,
            validTo = ticket.ValidTo
        });
    }

    #endregion
}
