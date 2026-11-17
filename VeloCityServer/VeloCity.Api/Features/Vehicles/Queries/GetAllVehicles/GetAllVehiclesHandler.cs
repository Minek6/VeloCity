using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Features.Vehicles.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Vehicles.Queries.GetAllVehicles;

public sealed class GetAllVehiclesHandler(
    ApplicationDbContext context)
    : IRequestHandler<
        GetAllVehiclesQuery,
        IEnumerable<VehicleDto>>
{
    public async Task<IEnumerable<VehicleDto>> Handle(
        GetAllVehiclesQuery request,
        CancellationToken ct)
    {
        return await context.Vehicles
            .AsNoTracking()
            .OrderBy(v => v.SideNumber)
            .Select(v => new VehicleDto
            (
                v.Id,
                v.SideNumber,
                v.Model,
                v.IsActive
            ))
            .ToListAsync(ct);
    }
}
