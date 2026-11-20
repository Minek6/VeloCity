using MediatR;
using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Common.Pagination;
using VeloCity.Api.Features.Lines.DTOs;
using VeloCity.Api.Models.Data;

namespace VeloCity.Api.Features.Lines.Queries.GetLines;

public class GetLinesHandler(ApplicationDbContext context)
    : IRequestHandler<GetLinesQuery, PaginatedList<LineDto>>
{
    public async Task<PaginatedList<LineDto>> Handle(
        GetLinesQuery request,
        CancellationToken ct)
    {
        var query = context.Lines.AsNoTracking().AsQueryable();

        // Filtrowanie statusu
        if (!request.IncludeInactive)
        {
            query = query.Where(l => l.IsActive);
        }
        else if (request.IsActive.HasValue)
        {
            query = query.Where(
                l => l.IsActive == request.IsActive.Value);
        }

        // Wyszukiwanie po nazwie
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();

            query = query.Where(l =>
                EF.Functions.ILike(
                    l.Name,
                    $"%{search}%"));
        }

        // Sortowanie
        query = request.IsDescending
            ? query.OrderByDescending(l => l.Name)
            : query.OrderBy(l => l.Name);

        // Mapowanie DTO
        var dtoQuery = query.Select(l =>
            new LineDto(
                l.Id,
                l.Name,
                l.IsActive));

        // Paginacja
        return await PaginatedList<LineDto>.CreateAsync(
            dtoQuery,
            request.PageNumber,
            request.PageSize,
            ct);
    }
}
