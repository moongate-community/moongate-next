namespace Moongate.Server.Data.World;

/// <summary>
///     Holds the mounted display item ids loaded from server asset data.
/// </summary>
public sealed class MountTileData
{
    private readonly HashSet<int> _itemIds = [];

    public IReadOnlySet<int> ItemIds => _itemIds;

    public bool Contains(int itemId)
    {
        return _itemIds.Contains(itemId);
    }

    public void Replace(IEnumerable<int> itemIds)
    {
        ArgumentNullException.ThrowIfNull(itemIds);

        _itemIds.Clear();

        foreach (var itemId in itemIds.Distinct())
        {
            _itemIds.Add(itemId);
        }
    }
}
