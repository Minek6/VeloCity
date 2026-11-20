using System.ComponentModel.DataAnnotations;

namespace VeloCity.Admin.Models.Forms;

public sealed class LineFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Podaj nazwę lub numer linii.")]
    [StringLength(30, MinimumLength = 1,
        ErrorMessage = "Nazwa może mieć maksymalnie 30 znaków.")]
    public string Name { get; set; } = string.Empty;
}
