using MediatR;

namespace VeloCity.Api.Features.Tickets.Commands.PurchaseTicket;

public record PurchaseTicketCommand(
    int TicketTypeId,
    int? ScheduledTripId = null,
    DateOnly? ServiceDate = null,
    int? StartStopId = null,
    int? EndStopId = null
) : IRequest<int>;