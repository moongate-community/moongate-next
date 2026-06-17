using Moongate.Core.Interfaces.Ids;
using Moongate.Persistence.Data;
using Moongate.Persistence.Interfaces.Persistence;

namespace Moongate.Persistence.Access;

/// <summary>
///     Base class that turns an <see cref="IAutoDataAccess{TEntity,TKey}" /> into a
///     paginated, searchable service. Subclasses define entity-specific search and
///     ordering; reads run synchronously over the in-memory query snapshot.
/// </summary>
public abstract class PaginatedEntityService<TEntity, TKey> : IPaginatedService<TEntity>
    where TKey : struct, IAutoIncrementKey<TKey>
{
    protected PaginatedEntityService(IAutoDataAccess<TEntity, TKey> data)
    {
        Data = data;
    }

    protected IAutoDataAccess<TEntity, TKey> Data { get; }

    public ValueTask<PagedResult<TEntity>> ListAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = Data.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = ApplySearch(query, request.Search);
        }

        query = ApplyOrder(query);

        var total = query.Count();
        var items = query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return ValueTask.FromResult(new PagedResult<TEntity>(items, request.Page, request.PageSize, total));
    }

    /// <summary>Applies a stable default ordering. Override to customize.</summary>
    protected virtual IQueryable<TEntity> ApplyOrder(IQueryable<TEntity> query)
    {
        return query;
    }

    /// <summary>Applies an entity-specific case-insensitive search filter.</summary>
    protected abstract IQueryable<TEntity> ApplySearch(IQueryable<TEntity> query, string term);
}
