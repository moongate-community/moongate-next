using Moongate.Persistence.Data;

namespace Moongate.Server.Data.ListQueries;

public static class InMemoryListQuery
{
    public static PagedResult<T> Apply<T>(
        IEnumerable<T> source,
        PageRequest request,
        Func<T, IEnumerable<string?>> searchableFields,
        IReadOnlyCollection<Func<T, bool>> filters
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(searchableFields);
        ArgumentNullException.ThrowIfNull(filters);

        var query = source;

        foreach (var filter in filters)
        {
            query = query.Where(filter);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(item => MatchesSearch(item, request.Search, searchableFields));
        }

        var filtered = query.ToArray();
        var pageItems = filtered.Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArray();

        return new PagedResult<T>(pageItems, request.Page, request.PageSize, filtered.Length);
    }

    private static bool MatchesSearch<T>(
        T item,
        string search,
        Func<T, IEnumerable<string?>> searchableFields
    )
    {
        return searchableFields(item)
            .Any(field => !string.IsNullOrWhiteSpace(field) && field.Contains(search, StringComparison.OrdinalIgnoreCase));
    }
}
