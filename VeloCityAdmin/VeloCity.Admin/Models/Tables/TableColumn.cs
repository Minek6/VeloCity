using Microsoft.AspNetCore.Components;

namespace VeloCity.Admin.Models.Tables;

public sealed class TableColumn<TItem>
{
    public required string Title { get; init; }

    public required Func<TItem, string> Value { get; init; }

    public Func<TItem, IComparable?>? SortValue { get; init; }

    public string? SortKey { get; init; }

    public RenderFragment<TItem>? Template { get; init; }
}
