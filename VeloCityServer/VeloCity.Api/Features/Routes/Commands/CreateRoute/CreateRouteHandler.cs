using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Models;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Routes.Commands.CreateRoute;

public class CreateRouteHandler(ApplicationDbContext context)
    : IRequestHandler<CreateRouteCommand, int>
{
    public async Task<int> Handle(
        CreateRouteCommand request,
        CancellationToken ct)
    {
        var lineExists = await context.Lines
            .AnyAsync(l =>
                l.Id == request.LineId && l.IsActive,
                ct);

        if (!lineExists)
        {
            throw new NotFoundException("Line", request.LineId);
        }

        var route = new Models.Route
        {
            LineId = request.LineId,
            Name = request.Name.Trim(),
            Direction = request.Direction,
            IsActive = true
        };

        context.Routes.Add(route);

        await context.SaveChangesAsync(ct);

        return route.Id;
    }
}
