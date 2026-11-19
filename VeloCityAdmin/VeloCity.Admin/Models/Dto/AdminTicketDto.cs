namespace VeloCity.Admin.Models.Dto;

public sealed class AdminTicketDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string UserSurname { get; set; } = string.Empty;

    public string TicketTypeName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public DateTime PurchasedAt { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public int? VehicleId { get; set; }

    public bool IsValidated { get; set; }
}
