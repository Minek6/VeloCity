using System.ComponentModel.DataAnnotations;

namespace VeloCity.Admin.Models.Forms;

public sealed class UpdateTicketValidityFormModel
{
    [Required(ErrorMessage = "Podaj nową datę ważności.")]
    public DateTime? ValidToLocal { get; set; }

    [Required(ErrorMessage = "Podaj powód korekty.")]
    [MinLength(5, ErrorMessage = "Powód musi mieć co najmniej 5 znaków.")]
    [MaxLength(500, ErrorMessage = "Powód może mieć maksymalnie 500 znaków.")]
    public string Reason { get; set; } = string.Empty;
}
