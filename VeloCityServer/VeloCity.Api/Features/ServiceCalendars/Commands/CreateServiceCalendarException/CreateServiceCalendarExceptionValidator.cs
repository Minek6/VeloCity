using FluentValidation;

namespace VeloCity.Api.Features.ServiceCalendars.Commands.CreateServiceCalendarException;

public class CreateServiceCalendarExceptionValidator
    : AbstractValidator<CreateServiceCalendarExceptionCommand>
{
    public CreateServiceCalendarExceptionValidator()
    {
        RuleFor(x => x.ServiceCalendarId)
            .GreaterThan(0)
            .WithMessage("ServiceCalendarId must be greater than zero.");

        RuleFor(x => x.Date)
            .Must(date => date != default(DateOnly))
            .WithMessage("Exception date is required.");
    }
}
