namespace VeloCity.Admin.Models;

public sealed record LoginResponse(
    string Token,
    string Name,
    string Role
);