using MediatR;
using VeloCity.Api.Features.Dashboard.DTOs;

namespace VeloCity.Api.Features.Dashboard.Queries.GetDashboardStats;

public record GetDashboardStatsQuery()
    : IRequest<DashboardStatsDto>;
