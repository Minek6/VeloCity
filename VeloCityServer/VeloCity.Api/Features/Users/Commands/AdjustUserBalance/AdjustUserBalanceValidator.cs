using FluentValidation;

namespace VeloCity.Api.Features.Users.Commands.AdjustUserBalance;

public sealed class AdjustUserBalanceValidator
    : AbstractValidator<AdjustUserBalanceCommand>
{
    public AdjustUserBalanceValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.Amount)
            .NotEqual(0)
            .WithMessage("Adjustment amount cannot be zero.");

        RuleFor(x => x.Amount)
            .Must(amount => Math.Abs(amount) <= 10000m)
            .WithMessage(
                "Maximum adjustment amount is 10000 PLN.");

        RuleFor(x => x.Amount)
            .Must(amount =>
                decimal.Round(amount, 2) == amount)
            .WithMessage(
                "Amount can have maximum 2 decimal places.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(500);
    }
}