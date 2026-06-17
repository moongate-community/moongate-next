namespace Moongate.Persistence.Data;

/// <summary>
///     One page of results plus the metadata a client needs to paginate.
/// </summary>
public sealed record PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }

    public int TotalPages => PageSize <= 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;

    /// <summary>Projects each item while preserving the page metadata.</summary>
    public PagedResult<TOut> Select<TOut>(Func<T, TOut> map)
    {
        var mapped = new List<TOut>(Items.Count);

        foreach (var item in Items)
        {
            mapped.Add(map(item));
        }

        return new PagedResult<TOut>(mapped, Page, PageSize, TotalCount);
    }
}
