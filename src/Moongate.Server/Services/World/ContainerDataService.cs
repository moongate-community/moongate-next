using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World.Internal;
using Moongate.Server.Services.WorldData;

namespace Moongate.Server.Services.World;

/// <summary>
/// Lazy in-memory store for container defaults and layouts.
/// </summary>
public class ContainerDataService : LazyDataService, IContainerDataService
{
    private readonly ServerAssetDataLoader? _loader;
    private readonly Lock _sync = new();
    private List<ContainerEntry> _containers = [];
    private List<ContainerLayoutEntry> _layouts = [];

    public ContainerDataService() { }

    public ContainerDataService(ServerAssetDataLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        _loader = loader;
    }

    public IReadOnlyList<ContainerEntry> GetAllContainers()
    {
        EnsureLoaded();

        lock (_sync)
        {
            return [.. _containers];
        }
    }

    public IReadOnlyList<ContainerLayoutEntry> GetAllLayouts()
    {
        EnsureLoaded();

        lock (_sync)
        {
            return [.. _layouts];
        }
    }

    public void SetContainers(IReadOnlyList<ContainerEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var snapshot = entries.ToList();

        lock (_sync)
        {
            _containers = snapshot;
        }

        MarkLoaded();
    }

    public void SetLayouts(IReadOnlyList<ContainerLayoutEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var snapshot = entries.ToList();

        lock (_sync)
        {
            _layouts = snapshot;
        }

        MarkLoaded();
    }

    protected override void LoadCore()
        => _loader?.LoadContainers(this);
}
