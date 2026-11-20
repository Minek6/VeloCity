using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Pagination;
using VeloCity.Api.Features.Stops.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Stops.Queries.GetStops;

public class GetStopsHandler(ApplicationDbContext context) : IRequestHandler<GetStopsQuery, PaginatedList<StopDto>>
{
    public async Task<PaginatedList<StopDto>> Handle(GetStopsQuery request, CancellationToken ct)
    {
        
        var query = context.Stops.AsNoTracking().AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(s => s.IsActive);
        }
        else if (request.IsActive.HasValue)
        {
            query = query.Where(
                s => s.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(s => s.Name.ToLower().Contains(request.SearchTerm.ToLower()));
        }

        query = request.IsDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name);
        var dtoQuery = query.Select(s => new StopDto(
            s.Id, s.Name, s.Latitude, s.Longitude, s.ZoneId, s.ExternalId, s.IsActive));

        return await PaginatedList<StopDto>.CreateAsync(dtoQuery, request.PageNumber, request.PageSize, ct);
    }
}
