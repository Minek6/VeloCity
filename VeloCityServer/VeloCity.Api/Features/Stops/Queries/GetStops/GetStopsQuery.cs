using MediatR;
using VeloCity.Api.Common.Pagination;
using VeloCity.Api.Features.Stops.DTOs;

namespace VeloCity.Api.Features.Stops.Queries.GetStops;

public class GetStopsQuery : PaginatedRequest, IRequest<PaginatedList<StopDto>>
{
    public string? SearchTerm { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public bool? IsActive { get; set; }
}
