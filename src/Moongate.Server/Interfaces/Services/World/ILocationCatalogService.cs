using Moongate.Server.Data.World;

namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
/// Provides access to flattened world location catalog entries.
/// </summary>
public interface ILocationCatalogService : IDataService
{
    /// <summary>
    /// Returns all loaded location entries.
    /// </summary>
    /// <returns>All loaded locations.</returns>
    IReadOnlyList<WorldLocationEntry> GetAllLocations();

    /// <summary>
    /// Replaces all currently loaded location entries.
    /// </summary>
    /// <param name="locations">Location entries.</param>
    void SetLocations(IReadOnlyList<WorldLocationEntry> locations);
}
