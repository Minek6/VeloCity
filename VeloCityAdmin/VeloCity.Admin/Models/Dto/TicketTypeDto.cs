namespace VeloCity.Admin.Models.Dto;

public sealed class TicketTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationInMinutes { get; set; }
    public int ZoneLimit { get; set; }
    public bool IsActive { get; set; }
}
