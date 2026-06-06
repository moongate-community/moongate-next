using Moongate.Server.Data.World;

namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
/// Provides access to name groups loaded from server asset data.
/// </summary>
public interface INameDataService
{
    /// <summary>
    /// Returns all loaded name groups.
    /// </summary>
    /// <returns>All loaded name groups.</returns>
    IReadOnlyList<NameGroupEntry> GetAllGroups();

    /// <summary>
    /// Replaces all currently loaded name groups.
    /// </summary>
    /// <param name="groups">Name groups.</param>
    void SetGroups(IReadOnlyList<NameGroupEntry> groups);
}
