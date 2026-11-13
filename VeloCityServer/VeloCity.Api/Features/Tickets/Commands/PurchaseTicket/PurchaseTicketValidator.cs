
using FluentValidation;

namespace VeloCity.Api.Features.Tickets.Commands.PurchaseTicket;

public class PurchaseTicketValidator
    : AbstractValidator<PurchaseTicketCommand>
{
    public PurchaseTicketValidator()
    {
        RuleFor(x => x.TicketTypeId)
            .GreaterThan(0)
            .WithMessage("Ticket Type ID must be greater than zero.");

        RuleFor(x => x.ScheduledTripId)
            .GreaterThan(0)
            .When(x => x.ScheduledTripId.HasValue);

        RuleFor(x => x.StartStopId)
            .GreaterThan(0)
            .When(x => x.StartStopId.HasValue);

        RuleFor(x => x.EndStopId)
            .GreaterThan(0)
            .When(x => x.EndStopId.HasValue);

        RuleFor(x => x)
            .Must(x =>
            {
                bool hasTrip = x.ScheduledTripId.HasValue;
                bool hasDate = x.ServiceDate.HasValue;
                bool hasStart = x.StartStopId.HasValue;
                bool hasEnd = x.EndStopId.HasValue;

                return (!hasTrip && !hasDate && !hasStart && !hasEnd)
                    || (hasTrip && hasDate && hasStart && hasEnd);
            })
            .WithMessage(
                "Selected connection requires trip, date and both stops.");

        RuleFor(x => x)
            .Must(x => x.StartStopId != x.EndStopId)
            .When(x => x.ScheduledTripId.HasValue)
            .WithMessage("Start and end stops must be different.");
    }
}
