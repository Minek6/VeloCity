using System.ComponentModel.DataAnnotations;
using VeloCity.Admin.Models.Enums;

namespace VeloCity.Admin.Models.Forms;

public sealed class CreateUserFormModel
{
    [Required(ErrorMessage = "Podaj imię.")]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Podaj nazwisko.")]
    [StringLength(50)]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Podaj adres e-mail.")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy adres e-mail.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Podaj hasło.")]
    [MinLength(8, ErrorMessage = "Hasło musi mieć minimum 8 znaków.")]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*[0-9]).+$",
        ErrorMessage = "Hasło musi zawierać wielką literę i cyfrę.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potwierdź hasło.")]
    [Compare(nameof(Password),
        ErrorMessage = "Hasła muszą być identyczne.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Passenger;
}
