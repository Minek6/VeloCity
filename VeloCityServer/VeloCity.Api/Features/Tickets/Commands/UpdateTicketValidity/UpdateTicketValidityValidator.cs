using FluentValidation;

namespace VeloCity.Api.Features.Tickets.Commands.UpdateTicketValidity;

public sealed class UpdateTicketValidityValidator
    : AbstractValidator<UpdateTicketValidityCommand>
{
    public UpdateTicketValidityValidator()
    {
        RuleFor(x => x.TicketId)
            .GreaterThan(0);

        RuleFor(x => x.ValidTo)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(500);
    }
}
