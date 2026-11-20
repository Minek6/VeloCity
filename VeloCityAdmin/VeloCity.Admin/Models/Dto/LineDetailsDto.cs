namespace VeloCity.Admin.Models.Dto;

public sealed class LineDetailsDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<LineRouteStopDto> Stops { get; set; } = [];
}

public sealed class LineRouteStopDto
{
    public int StopId { get; set; }

    public string StopName { get; set; } = string.Empty;

    public int Sequence { get; set; }

    public int Direction { get; set; }
}
