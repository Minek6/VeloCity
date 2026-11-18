using MediatR;
using VeloCity.Api.Models.Enums;

namespace VeloCity.Api.Features.Users.Commands.AdminCreateUser;

public record AdminCreateUserCommand(
    string Name,
    string Surname,
    string Email,
    string Password,
    UserRole Role
) : IRequest<int>;
