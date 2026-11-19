using MediatR;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Tickets.Commands.ActivateTicketType;

public sealed class ActivateTicketTypeHandler(
    ApplicationDbContext context)
    : IRequestHandler<ActivateTicketTypeCommand>
{
    public async Task Handle(
        ActivateTicketTypeCommand request,
        CancellationToken ct)
    {
        var ticketType = await context.TicketTypes
            .FindAsync([request.Id], ct)
            ?? throw new NotFoundException(
                "TicketType", request.Id);

        if (ticketType.IsActive)
        {
            throw new AppException(
                "Ticket type is already active.", 400);
        }

        ticketType.IsActive = true;

        await context.SaveChangesAsync(ct);
    }
}
