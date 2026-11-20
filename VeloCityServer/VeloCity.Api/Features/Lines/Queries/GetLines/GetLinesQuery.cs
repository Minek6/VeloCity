using MediatR;
using VeloCity.Api.Common.Pagination;
using VeloCity.Api.Features.Lines.DTOs;

namespace VeloCity.Api.Features.Lines.Queries.GetLines;

public record GetLinesQuery(
    string? SearchTerm,
    bool IsDescending,
    int PageNumber,
    int PageSize,
    bool IncludeInactive = false,
    bool? IsActive = null
) : IRequest<PaginatedList<LineDto>>;
