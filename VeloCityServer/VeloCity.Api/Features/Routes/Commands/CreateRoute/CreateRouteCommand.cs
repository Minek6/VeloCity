using MediatR;

namespace VeloCity.Api.Features.Routes.Commands.CreateRoute;

public record CreateRouteCommand(
    int LineId,
    string Name,
    int Direction
) : IRequest<int>;
