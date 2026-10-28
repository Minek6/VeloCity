using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeloCity.Api.Features.Vehicles.Commands.CreateVehicle;
using VeloCity.Api.Features.Vehicles.Commands.DeleteVehicle;
using VeloCity.Api.Features.Vehicles.Commands.UpdateVehicle;
using VeloCity.Api.Features.Vehicles.DTOs;
using VeloCity.Api.Features.Vehicles.Queries.GetVehicleById;
using VeloCity.Api.Features.Vehicles.Queries.GetVehicles;
using VeloCity.Api.Models.Enums;

namespace VeloCity.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class VehiclesController(IMediator mediator) : ControllerBase
{
    // ADMIN ONLY: create
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost()]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateVehicleCommand command)
    {
        var vehicle = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
    }

    // ADMIN ONLY: get vehicle by id
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var vehicle = await mediator.Send(new GetVehicleByIdQuery(id));
        return Ok(vehicle);
    }

    // ADMIN ONLY: get all vehicles
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var vehicles = await mediator.Send(new GetVehiclesQuery());
        return Ok(vehicles);
    }

    // ADMIN ONLY: update vehicle
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch.");

        await mediator.Send(command);
        return NoContent();
    }

    // ADMIN ONLY: delete vehicle by id
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteVehicleCommand(id));
        return NoContent();
    }
}
