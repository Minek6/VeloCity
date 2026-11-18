namespace VeloCity.Admin.Models.Dto;

public sealed class BalanceAdjustmentDto
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
