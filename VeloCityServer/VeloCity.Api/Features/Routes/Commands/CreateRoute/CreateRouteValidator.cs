using FluentValidation;

namespace VeloCity.Api.Features.Routes.Commands.CreateRoute;

public class CreateRouteValidator
    : AbstractValidator<CreateRouteCommand>
{
    public CreateRouteValidator()
    {
        RuleFor(x => x.LineId)
            .GreaterThan(0)
            .WithMessage("LineId must be greater than zero.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Route name cannot be empty.")
            .MaximumLength(150)
            .WithMessage("Route name cannot exceed 150 characters.");

        RuleFor(x => x.Direction)
            .InclusiveBetween(0, 1)
            .WithMessage("Direction must be 0 or 1.");
    }
}
