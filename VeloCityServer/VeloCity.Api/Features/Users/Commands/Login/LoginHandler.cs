using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VeloCity.Api.Models.Data;
using VeloCity.Api.Common.Exceptions;

namespace VeloCity.Api.Features.Users.Commands.Login;

public class LoginHandler(
    ApplicationDbContext context,
    IConfiguration config)
    : IRequestHandler<LoginCommand, LoginResponse?>
{
    public async Task<LoginResponse?> Handle(LoginCommand request,
        CancellationToken ct)
    {
        // Get user from db
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email,
            ct);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(request.Password,
                user.PasswordHash))
            return null;

        if(!user.IsActive)
            throw new AppException("Konto użytkownika jest nieaktywne.", 403);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
        // token content
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier,
                    user.Id.ToString()),
                new Claim(ClaimTypes.Email,
                    user.Email),
                new Claim(ClaimTypes.Role,
                    user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return new LoginResponse(tokenHandler.WriteToken(token),
            user.Name,
            user.Role.ToString());
    }
}
