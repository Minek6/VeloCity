using MediatR;

namespace VeloCity.Api.Features.Stops.Commands.ActivateStop;

public record ActivateStopCommand(int Id) : IRequest;
