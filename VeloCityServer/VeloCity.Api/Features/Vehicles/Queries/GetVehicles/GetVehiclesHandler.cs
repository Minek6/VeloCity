using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Features.Vehicles.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Vehicles.Queries.GetVehicles;

public class GetVehiclesHandler(
    ApplicationDbContext context) : IRequestHandler<GetVehiclesQuery, IEnumerable<VehicleDto>>
{
    public async Task<IEnumerable<VehicleDto>> Handle(GetVehiclesQuery request,
        CancellationToken ct)
    {
        return await context.Vehicles
            .AsNoTracking()
            .Where(v => v.IsActive) //ACTIVE VEHICLES ONLY
            .Select(v => new VehicleDto(
                v.Id,
                v.SideNumber,
                v.Model,
                v.IsActive
            )).ToListAsync(ct);
    }
}
