using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Stops.Commands.ActivateStop;

public sealed class ActivateStopHandler(
    ApplicationDbContext context)
    : IRequestHandler<ActivateStopCommand>
{
    public async Task Handle(
        ActivateStopCommand request,
        CancellationToken ct)
    {
        var stop = await context.Stops
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

        if (stop is null)
            throw new NotFoundException("Stop", request.Id);

        if (stop.IsActive)
            return;

        stop.IsActive = true;

        await context.SaveChangesAsync(ct);
    }
}
