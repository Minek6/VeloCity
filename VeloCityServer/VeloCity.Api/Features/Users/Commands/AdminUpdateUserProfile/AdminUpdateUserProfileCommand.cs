using MediatR;

namespace VeloCity.Api.Features.Users.Commands.AdminUpdateUserProfile;

public record AdminUpdateUserProfileCommand(
    int UserId,
    string Name,
    string Surname,
    string Email
) : IRequest;
