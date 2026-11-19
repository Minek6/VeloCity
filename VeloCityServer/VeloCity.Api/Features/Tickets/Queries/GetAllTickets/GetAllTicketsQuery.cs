using MediatR;
using VeloCity.Api.Common.Pagination;
using VeloCity.Api.Features.Tickets.DTOs;

namespace VeloCity.Api.Features.Tickets.Queries.GetAllTickets;

public class GetAllTicketsQuery : PaginatedRequest, IRequest<PaginatedList<AdminTicketDto>>
{
    public int? UserId { get; set; }
    public int? VehicleId { get; set; }
    public bool? IsValidated { get; set; }
}
