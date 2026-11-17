using MediatR;
using VeloCity.Api.Common.Exceptions;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Vehicles.Commands.ActivateVehicle;

public sealed class ActivateVehicleHandler(
    ApplicationDbContext context)
    : IRequestHandler<ActivateVehicleCommand>
{
    public async Task Handle(
        ActivateVehicleCommand request,
        CancellationToken ct)
    {
        var vehicle = await context.Vehicles
            .FindAsync([request.Id], ct)
            ?? throw new NotFoundException("Vehicle", request.Id);

        if (vehicle.IsActive)
        {
            throw new AppException(
                "Vehicle is already active.", 400);
        }

        vehicle.IsActive = true;

        await context.SaveChangesAsync(ct);
    }
}
