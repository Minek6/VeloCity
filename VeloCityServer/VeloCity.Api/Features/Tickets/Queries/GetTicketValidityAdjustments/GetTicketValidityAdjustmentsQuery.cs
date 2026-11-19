using MediatR;
using VeloCity.Api.Features.Tickets.DTOs;

namespace VeloCity.Api.Features.Tickets.Queries.GetTicketValidityAdjustments;

public record GetTicketValidityAdjustmentsQuery(int TicketId)
    : IRequest<List<TicketValidityAdjustmentDto>>;
