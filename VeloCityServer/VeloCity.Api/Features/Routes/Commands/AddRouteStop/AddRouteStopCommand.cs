using MediatR;

namespace VeloCity.Api.Features.Routes.Commands.AddRouteStop;

public record AddRouteStopCommand(
    int RouteId,
    int StopId,
    int Sequence
) : IRequest<int>;
