using MediatR;
using VeloCity.Api.Features.Dashboard.DTOs;

namespace VeloCity.Api.Features.Dashboard.Queries.GetTicketSalesReport;

public record GetTicketSalesReportQuery(
    DateOnly From,
    DateOnly To,
    int? TicketTypeId,
    bool? IsValidated
) : IRequest<List<DailyTicketSalesDto>>;
