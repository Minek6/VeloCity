using MediatR;
using VeloCity.Api.Features.Dashboard.DTOs;

namespace VeloCity.Api.Features.Dashboard.Queries.GetTicketSalesChart;

public record GetTicketSalesChartQuery(int Days = 7)
    : IRequest<List<DailyTicketSalesDto>>;
