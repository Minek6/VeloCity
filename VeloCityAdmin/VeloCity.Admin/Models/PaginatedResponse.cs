namespace VeloCity.Admin.Models;

public sealed class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = [];

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
