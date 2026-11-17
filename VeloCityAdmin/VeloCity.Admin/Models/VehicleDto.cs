namespace VeloCity.Admin.Models;

public sealed class VehicleDto
{
    public int Id { get; set; }
    public string SideNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
