using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeloCity.Api.Common.DTOs;
using VeloCity.Api.Common.Pagination;
using VeloCity.Api.Features.Users.Commands.ChangePassword;
using VeloCity.Api.Features.Users.Commands.Login;
using VeloCity.Api.Features.Users.Commands.CreateUser;
using VeloCity.Api.Features.Users.Commands.DeleteOwnAccount;
using VeloCity.Api.Features.Users.Commands.DeleteUser;
using VeloCity.Api.Features.Users.Commands.UpdateProfile;
using VeloCity.Api.Features.Users.Commands.UpdateUserStatus;
using VeloCity.Api.Features.Users.Queries.GetProfile;
using VeloCity.Api.Features.Users.Queries.GetUserDetails;
using VeloCity.Api.Features.Users.Queries.GetUsers;
using VeloCity.Api.Features.Users.Commands.AdminUpdateUserProfile;
using VeloCity.Api.Features.Users.Commands.AdjustUserBalance;
using VeloCity.Api.Features.Users.Commands.AdminCreateUser;
using VeloCity.Api.Models.Enums;

namespace VeloCity.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController(
    IMediator mediator) : ControllerBase
{

    // login
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        LoginResponse? response = await mediator.Send(command);

        if (response == null)
            return Unauthorized(new
            {
                message = "Wrong username or password"
            });

        return Ok(response);
    }

    // register
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
    {
        var userId = await mediator.Send(command);

        return CreatedAtAction(
            nameof(GetProfile),
            new { id = userId },
            new { id = userId, message = "User added successfully" }
        );
    }

    // get user profile
    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await mediator.Send(new GetProfileQuery());
        return Ok(profile);
    }

    // self delete user
    [Authorize]
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMe()
    {
        await mediator.Send(new DeleteOwnAccountCommand());
        return NoContent();
    }

    // update my user
    [Authorize]
    [HttpPut("me")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        var updatedProfile = await mediator.Send(command);
        return Ok(updatedProfile);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id:int}/profile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AdminUpdateProfile(
        int id,
        [FromBody] AdminUpdateProfileRequest request)
    {
        await mediator.Send(
            new AdminUpdateUserProfileCommand(
                id,
                request.Name,
                request.Surname,
                request.Email));

        return NoContent();
    }

    public record AdminUpdateProfileRequest(
        string Name,
        string Surname,
        string Email);


    // update my password
    [Authorize]
    [HttpPut("me/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        await mediator.Send(command);
        return NoContent();
    }

    // ADMIN ONLY: delete user
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }

    // ADMIN ONLY: get all user
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result);
    }

    // ADMIN ONLY: get user details
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetails(int id)
    {
        var result = await mediator.Send(new GetUserDetailsQuery(id));
        return Ok(result);
    }

    // ADMIN ONLY: update user role
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateStatusRequest body)
    {
        var command = new UpdateUserStatusCommand(id, body.Role, body.IsActive);
        await mediator.Send(command);
        return NoContent();
    }

    // ADMIN ONLY: adjust user balance
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{id:int}/balance-adjustments")]
    [ProducesResponseType(
        typeof(BalanceAdjustmentDto),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> AdjustBalance(
        int id,
        [FromBody] AdjustBalanceRequest request)
    {
        var result = await mediator.Send(
            new AdjustUserBalanceCommand(
                id,
                request.Amount,
                request.Reason));

        return Ok(result);
    }

    // ADMIN ONLY: create user with selected role
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdminCreateUser(
        [FromBody] AdminCreateUserCommand command)
    {
        var userId = await mediator.Send(command);

        return CreatedAtAction(
            nameof(GetUserDetails),
            new { id = userId },
            new
            {
                id = userId,
                message = "User created successfully."
            });
    }

}

public record AdjustBalanceRequest(
    decimal Amount,
    string Reason
);
