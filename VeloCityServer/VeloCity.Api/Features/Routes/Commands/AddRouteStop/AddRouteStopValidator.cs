using FluentValidation;

namespace VeloCity.Api.Features.Routes.Commands.AddRouteStop;

public class AddRouteStopValidator
    : AbstractValidator<AddRouteStopCommand>
{
    public AddRouteStopValidator()
    {
        RuleFor(x => x.RouteId)
            .GreaterThan(0)
            .WithMessage("RouteId must be greater than zero.");

        RuleFor(x => x.StopId)
            .GreaterThan(0)
            .WithMessage("StopId must be greater than zero.");

        RuleFor(x => x.Sequence)
            .GreaterThan(0)
            .WithMessage("Sequence must be greater than zero.");
    }
}
