using FluentValidation;

namespace VeloCity.Api.Features.Users.Commands.AdminUpdateUserProfile;

public sealed class AdminUpdateUserProfileValidator
    : AbstractValidator<AdminUpdateUserProfileCommand>
{
    public AdminUpdateUserProfileValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Surname)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);
    }
}
