using Moongate.Server.Data.World;

namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
/// Provides access to region definitions loaded from server asset data.
/// </summary>
public interface IRegionDataService : IDataService
{
    /// <summary>
    /// Returns all loaded region entries.
    /// </summary>
    /// <returns>All region entries.</returns>
    IReadOnlyList<RegionEntry> GetAllEntries();

    /// <summary>
    /// Replaces all currently loaded region entries.
    /// </summary>
    /// <param name="entries">Region entries.</param>
    void SetEntries(IReadOnlyList<RegionEntry> entries);
}
