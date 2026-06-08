namespace Moongate.Persistence.Data;

/// <summary>
/// A normalized paging and search request: 1-based page, bounded page size,
/// and an optional trimmed search term.
/// </summary>
public sealed record PageRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Page { get; }
    public int PageSize { get; }
    public string? Search { get; }

    public PageRequest(int page, int pageSize, string? search)
    {
        Page = page;
        PageSize = pageSize;
        Search = search;
    }

    /// <summary>
    /// Builds a request with page clamped to >= 1, page size clamped to
    /// 1..<see cref="MaxPageSize" /> (null defaults to <see cref="DefaultPageSize" />),
    /// and a blank search collapsed to null.
    /// </summary>
    public static PageRequest Normalize(int? page, int? pageSize, string? search)
    {
        var safePage = page is null or < 1 ? 1 : page.Value;
        var requestedSize = pageSize ?? DefaultPageSize;
        var safeSize = requestedSize < 1 ? 1 : requestedSize > MaxPageSize ? MaxPageSize : requestedSize;
        var safeSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        return new PageRequest(safePage, safeSize, safeSearch);
    }
}
