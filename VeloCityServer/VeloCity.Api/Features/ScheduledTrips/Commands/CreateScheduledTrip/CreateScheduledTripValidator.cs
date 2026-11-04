
using FluentValidation;

namespace VeloCity.Api.Features.ScheduledTrips.Commands.CreateScheduledTrip;

public class CreateScheduledTripValidator
    : AbstractValidator<CreateScheduledTripCommand>
{
    public CreateScheduledTripValidator()
    {
        RuleFor(x => x.RouteId)
            .GreaterThan(0)
            .WithMessage("RouteId must be greater than zero.");

        RuleFor(x => x.ServiceCalendarId)
            .GreaterThan(0)
            .WithMessage("ServiceCalendarId must be greater than zero.");

        RuleFor(x => x.StopTimes)
            .NotNull()
            .Must(x => x is { Count: > 0 })
            .WithMessage("At least one stop time is required.");

        RuleFor(x => x.StopTimes)
            .Must(x => x == null ||
                x.Select(st => st.RouteStopId).Distinct().Count() == x.Count)
            .WithMessage("RouteStopId values must be unique.");

        RuleForEach(x => x.StopTimes)
            .ChildRules(stop =>
            {
                stop.RuleFor(x => x.RouteStopId)
                    .GreaterThan(0);

                stop.RuleFor(x => x.ArrivalTime)
                    .GreaterThanOrEqualTo(TimeSpan.Zero)
                    .LessThan(TimeSpan.FromHours(48));

                stop.RuleFor(x => x.DepartureTime)
                    .GreaterThanOrEqualTo(x => x.ArrivalTime)
                    .WithMessage(
                        "Departure time cannot be earlier than arrival time.");

                stop.RuleFor(x => x.DepartureTime)
                    .LessThan(TimeSpan.FromHours(48));
            });
    }
}
