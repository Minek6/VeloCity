using System.ComponentModel.DataAnnotations;

namespace VeloCity.Admin.Models.Forms;

public sealed class StopFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Podaj nazwę przystanku.")]
    [StringLength(150, MinimumLength = 2,
        ErrorMessage = "Nazwa musi mieć od 2 do 150 znaków.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Podaj szerokość geograficzną.")]
    [Range(-90, 90,
        ErrorMessage = "Szerokość musi mieścić się od -90 do 90.")]
    public double? Latitude { get; set; }

    [Required(ErrorMessage = "Podaj długość geograficzną.")]
    [Range(-180, 180,
        ErrorMessage = "Długość musi mieścić się od -180 do 180.")]
    public double? Longitude { get; set; }

    [Required(ErrorMessage = "Podaj numer strefy.")]
    [Range(0, int.MaxValue,
        ErrorMessage = "Strefa nie może być ujemna.")]
    public int? Zone { get; set; } = 0;

    [Required(ErrorMessage = "Podaj identyfikator zewnętrzny.")]
    [Range(0, int.MaxValue,
        ErrorMessage = "Identyfikator nie może być ujemny.")]
    public int? ExternalId { get; set; }
}
