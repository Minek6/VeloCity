using MediatR;
using VeloCity.Api.Features.Dashboard.DTOs;

namespace VeloCity.Api.Features.Dashboard.Queries.GetSystemHealth;

public record GetSystemHealthQuery()
    : IRequest<SystemHealthDto>;
