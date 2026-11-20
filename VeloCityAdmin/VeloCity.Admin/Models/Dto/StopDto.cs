namespace VeloCity.Admin.Models.Dto;

public sealed class StopDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int Zone { get; set; }

    public int? ExternalId { get; set; }

    public bool IsActive { get; set; }
}
