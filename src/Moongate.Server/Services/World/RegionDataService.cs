using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World.Internal;
using Moongate.Server.Services.WorldData;

namespace Moongate.Server.Services.World;

/// <summary>
///     Lazy in-memory store for region entries.
/// </summary>
public class RegionDataService : LazyDataService, IRegionDataService
{
    private readonly ServerAssetDataLoader? _loader;
    private readonly Lock _sync = new();
    private List<RegionEntry> _entries = [];

    public RegionDataService()
    {
    }

    public RegionDataService(ServerAssetDataLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        _loader = loader;
    }

    public IReadOnlyList<RegionEntry> GetAllEntries()
    {
        EnsureLoaded();

        lock (_sync)
        {
            return [.. _entries];
        }
    }

    public void SetEntries(IReadOnlyList<RegionEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var snapshot = entries.ToList();

        lock (_sync)
        {
            _entries = snapshot;
        }

        MarkLoaded();
    }

    protected override void LoadCore()
    {
        _loader?.LoadRegions(this);
    }
}
