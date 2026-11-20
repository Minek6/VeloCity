namespace VeloCity.Admin.Models.Dto;

public sealed class SystemHealthDto
{
    public bool DatabaseConnected { get; set; }

    public long DatabaseResponseTimeMs { get; set; }

    public DateTime CheckedAtUtc { get; set; }
}
