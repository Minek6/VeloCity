using MediatR;
using VeloCity.Api.Features.Vehicles.DTOs;

namespace VeloCity.Api.Features.Vehicles.Queries.GetAllVehicles;

public record GetAllVehiclesQuery()
    : IRequest<IEnumerable<VehicleDto>>;