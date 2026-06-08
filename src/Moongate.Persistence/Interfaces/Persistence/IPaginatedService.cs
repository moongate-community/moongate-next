using Moongate.Persistence.Data;

namespace Moongate.Persistence.Interfaces.Persistence;

/// <summary>
/// A read service that returns a normalized, searchable page of entities.
/// </summary>
public interface IPaginatedService<TEntity>
{
    /// <summary>Returns one page of entities for the given request.</summary>
    ValueTask<PagedResult<TEntity>> ListAsync(PageRequest request, CancellationToken cancellationToken = default);
}
