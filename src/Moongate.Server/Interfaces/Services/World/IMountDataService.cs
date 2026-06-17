namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
///     Provides access to mount tile item ids loaded from server asset data.
/// </summary>
public interface IMountDataService : IDataService
{
    /// <summary>
    ///     Returns whether an item id is a known mount tile id.
    /// </summary>
    /// <param name="itemId">Item id.</param>
    /// <returns><c>true</c> when the item id is known as a mount; otherwise <c>false</c>.</returns>
    bool Contains(int itemId);

    /// <summary>
    ///     Returns all loaded mount tile item ids.
    /// </summary>
    /// <returns>All loaded mount tile item ids.</returns>
    IReadOnlySet<int> GetAllEntries();

    /// <summary>
    ///     Replaces all currently loaded mount tile item ids.
    /// </summary>
    /// <param name="itemIds">Mount tile item ids.</param>
    void SetEntries(IEnumerable<int> itemIds);
}
