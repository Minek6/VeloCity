using VeloCity.Admin.Models.Enums;
using System.Text.Json.Serialization;

namespace VeloCity.Admin.Models.Dto;

public sealed class UserDetailsDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public decimal Balance { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole Role { get; set; }
    
    public bool IsActive { get; set; }
}
