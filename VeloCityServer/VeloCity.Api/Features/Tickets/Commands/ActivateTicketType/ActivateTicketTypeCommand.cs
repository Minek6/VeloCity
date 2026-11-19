using MediatR;

namespace VeloCity.Api.Features.Tickets.Commands.ActivateTicketType;

public record ActivateTicketTypeCommand(int Id) : IRequest;
