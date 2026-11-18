using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Models;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Users.Commands.AdminCreateUser;

public sealed class AdminCreateUserHandler(
    ApplicationDbContext context)
    : IRequestHandler<AdminCreateUserCommand, int>
{
    public async Task<int> Handle(
        AdminCreateUserCommand request,
        CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // Sprawdzamy, czy adres e-mail jest już zajęty.
        var emailExists = await context.Users
            .AnyAsync(u => u.Email.ToLower() == email, ct);

        if (emailExists)
        {
            throw new AppException(
                "Email already exists.", 400);
        }

        // Hasło przechowujemy wyłącznie jako hash.
        var passwordHash =
            BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            Email = email,
            PasswordHash = passwordHash,
            Role = request.Role,
            Balance = 0.00m,
            IsActive = true
        };

        context.Users.Add(user);

        await context.SaveChangesAsync(ct);

        return user.Id;
    }
}
