using MediatR;
using VeloCity.Api.Features.Dashboard.DTOs;

namespace VeloCity.Api.Features.Dashboard.Queries.GetTicketTypeSalesRanking;

public record GetTicketTypeSalesRankingQuery(
    DateOnly From,
    DateOnly To,
    int? TicketTypeId,
    bool? IsValidated)
    : IRequest<List<TicketTypeSalesDto>>;
