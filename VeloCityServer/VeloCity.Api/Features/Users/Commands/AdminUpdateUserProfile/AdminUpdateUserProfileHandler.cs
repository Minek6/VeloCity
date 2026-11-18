using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Users.Commands.AdminUpdateUserProfile;

public sealed class AdminUpdateUserProfileHandler(
    ApplicationDbContext context)
    : IRequestHandler<AdminUpdateUserProfileCommand>
{
    public async Task Handle(
        AdminUpdateUserProfileCommand request,
        CancellationToken ct)
    {
        var user = await context.Users.FindAsync(
            [request.UserId], ct)
            ?? throw new NotFoundException(
                "User", request.UserId);

        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await context.Users
            .AnyAsync(u =>
                u.Id != request.UserId &&
                u.Email.ToLower() == email,
                ct);

        if (emailExists)
        {
            throw new AppException(
                "Email already exists.", 400);
        }

        user.Name = request.Name.Trim();
        user.Surname = request.Surname.Trim();
        user.Email = email;

        await context.SaveChangesAsync(ct);
    }
}
