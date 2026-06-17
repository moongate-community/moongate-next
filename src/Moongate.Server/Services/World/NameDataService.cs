using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World.Internal;
using Moongate.Server.Services.WorldData;

namespace Moongate.Server.Services.World;

/// <summary>
///     Lazy in-memory store for name groups.
/// </summary>
public class NameDataService : LazyDataService, INameDataService
{
    private readonly ServerAssetDataLoader? _loader;
    private readonly Lock _sync = new();
    private List<NameGroupEntry> _groups = [];

    public NameDataService()
    {
    }

    public NameDataService(ServerAssetDataLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        _loader = loader;
    }

    public IReadOnlyList<NameGroupEntry> GetAllGroups()
    {
        EnsureLoaded();

        lock (_sync)
        {
            return [.. _groups];
        }
    }

    public void SetGroups(IReadOnlyList<NameGroupEntry> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);

        var snapshot = groups.ToList();

        lock (_sync)
        {
            _groups = snapshot;
        }

        MarkLoaded();
    }

    protected override void LoadCore()
    {
        _loader?.LoadNames(this);
    }
}
