using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Server.Services.World;

/// <summary>
/// In-memory store for mount tile item ids loaded at startup.
/// </summary>
public class MountDataService : IMountDataService
{
    private readonly object _sync = new();
    private HashSet<int> _itemIds = [];

    public IReadOnlySet<int> GetAllEntries()
    {
        lock (_sync)
        {
            return new HashSet<int>(_itemIds);
        }
    }

    public bool Contains(int itemId)
    {
        lock (_sync)
        {
            return _itemIds.Contains(itemId);
        }
    }

    public void SetEntries(IEnumerable<int> itemIds)
    {
        ArgumentNullException.ThrowIfNull(itemIds);

        var snapshot = itemIds.Distinct().ToHashSet();

        lock (_sync)
        {
            _itemIds = snapshot;
        }
    }
}
