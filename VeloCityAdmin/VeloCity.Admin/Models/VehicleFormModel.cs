using System.ComponentModel.DataAnnotations;

namespace VeloCity.Admin.Models;

public sealed class VehicleFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Numer boczny jest wymagany.")]
    [StringLength(30, ErrorMessage = "Maksymalnie 30 znaków.")]
    public string SideNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Model jest wymagany.")]
    [StringLength(100, ErrorMessage = "Maksymalnie 100 znaków.")]
    public string Model { get; set; } = string.Empty;
}
