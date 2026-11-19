using System.ComponentModel.DataAnnotations;

namespace VeloCity.Admin.Models.Forms;

public sealed class CreateTicketTypeFormModel
{
    [Required(ErrorMessage = "Podaj nazwę biletu.")]
    [StringLength(100, ErrorMessage = "Nazwa może mieć maksymalnie 100 znaków.")]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999.99", ParseLimitsInInvariantCulture = true,
        ErrorMessage = "Cena musi być większa od 0 zł.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue,
        ErrorMessage = "Czas ważności nie może być ujemny.")]
    public int DurationInMinutes { get; set; }

    [Range(0, int.MaxValue,
        ErrorMessage = "Limit stref nie może być ujemny.")]
    public int ZoneLimit { get; set; }
}
