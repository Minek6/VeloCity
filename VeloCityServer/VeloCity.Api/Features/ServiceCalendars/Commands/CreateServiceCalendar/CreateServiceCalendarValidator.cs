using FluentValidation;

namespace VeloCity.Api.Features.ServiceCalendars.Commands.CreateServiceCalendar;

public class CreateServiceCalendarValidator
    : AbstractValidator<CreateServiceCalendarCommand>
{
    public CreateServiceCalendarValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Calendar name cannot be empty.")
            .MaximumLength(100)
            .WithMessage("Calendar name cannot exceed 100 characters.");

        RuleFor(x => x.ValidFrom)
            .LessThanOrEqualTo(x => x.ValidTo)
            .WithMessage("ValidFrom cannot be later than ValidTo.");

        RuleFor(x => x)
            .Must(x =>
                x.Monday ||
                x.Tuesday ||
                x.Wednesday ||
                x.Thursday ||
                x.Friday ||
                x.Saturday ||
                x.Sunday)
            .WithName("Weekdays")
            .WithMessage("At least one weekday must be selected.");
    }
}
