using MediatR;

namespace VeloCity.Api.Features.Lines.Commands.ActivateLine;

public record ActivateLineCommand(int Id) : IRequest;
