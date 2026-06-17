using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World.Internal;
using Moongate.Server.Services.WorldData;

namespace Moongate.Server.Services.World;

/// <summary>
///     Lazy in-memory store for mount tile item ids.
/// </summary>
public class MountDataService : LazyDataService, IMountDataService
{
    private readonly ServerAssetDataLoader? _loader;
    private readonly Lock _sync = new();
    private HashSet<int> _itemIds = [];

    public MountDataService()
    {
    }

    public MountDataService(ServerAssetDataLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        _loader = loader;
    }

    public bool Contains(int itemId)
    {
        EnsureLoaded();

        lock (_sync)
        {
            return _itemIds.Contains(itemId);
        }
    }

    public IReadOnlySet<int> GetAllEntries()
    {
        EnsureLoaded();

        lock (_sync)
        {
            return new HashSet<int>(_itemIds);
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

        MarkLoaded();
    }

    protected override void LoadCore()
    {
        _loader?.LoadMounts(this);
    }
}
