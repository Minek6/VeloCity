using MediatR;

namespace VeloCity.Api.Features.Tickets.Queries.GetAdminTicketTypes;

public record GetAdminTicketTypesQuery
    : IRequest<List<AdminTicketTypeDto>>;

public record AdminTicketTypeDto(
    int Id,
    string Name,
    decimal Price,
    int DurationInMinutes,
    int ZoneLimit,
    bool IsActive);
