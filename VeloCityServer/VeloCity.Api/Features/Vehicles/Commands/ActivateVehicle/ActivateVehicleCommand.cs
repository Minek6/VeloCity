using MediatR;

namespace VeloCity.Api.Features.Vehicles.Commands.ActivateVehicle;

public record ActivateVehicleCommand(int Id) : IRequest;