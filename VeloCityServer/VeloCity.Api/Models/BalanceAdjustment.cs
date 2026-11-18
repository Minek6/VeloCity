namespace VeloCity.Api.Models;

public class BalanceAdjustment
{
    public int Id { get; set; }

    // Użytkownik, którego saldo zmieniono
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Administrator wykonujący korektę
    public int AdminId { get; set; }
    public User Admin { get; set; } = null!;

    // Dodatnia lub ujemna kwota korekty
    public decimal Amount { get; set; }

    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    // Uzasadnienie korekty
    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
