using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Lines.Commands.ActivateLine;

public sealed class ActivateLineHandler(
    ApplicationDbContext context)
    : IRequestHandler<ActivateLineCommand>
{
    public async Task Handle(
        ActivateLineCommand request,
        CancellationToken ct)
    {
        var line = await context.Lines
            .FirstOrDefaultAsync(l => l.Id == request.Id, ct);

        if (line is null)
            throw new NotFoundException("Line", request.Id);

        if (line.IsActive)
            return;

        var duplicate = await context.Lines.AnyAsync(
            l => l.Id != line.Id &&
                 l.IsActive &&
                 l.Name.ToLower() == line.Name.ToLower(),
            ct);

        if (duplicate)
            throw new AppException(
                "An active line with this name already exists.", 400);

        line.IsActive = true;

        await context.SaveChangesAsync(ct);
    }
}
