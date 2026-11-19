using MediatR;

namespace VeloCity.Api.Features.Tickets.Commands.UpdateTicketValidity;

public record UpdateTicketValidityCommand(
    int TicketId,
    DateTime ValidTo,
    string Reason
) : IRequest;
