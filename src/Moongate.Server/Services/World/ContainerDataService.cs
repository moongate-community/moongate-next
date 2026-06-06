using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Server.Services.World;

/// <summary>
/// In-memory store for container defaults and layouts loaded at startup.
/// </summary>
public class ContainerDataService : IContainerDataService
{
    private readonly object _sync = new();
    private List<ContainerEntry> _containers = [];
    private List<ContainerLayoutEntry> _layouts = [];

    public IReadOnlyList<ContainerEntry> GetAllContainers()
    {
        lock (_sync)
        {
            return [.. _containers];
        }
    }

    public IReadOnlyList<ContainerLayoutEntry> GetAllLayouts()
    {
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
    }

    public void SetLayouts(IReadOnlyList<ContainerLayoutEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var snapshot = entries.ToList();

        lock (_sync)
        {
            _layouts = snapshot;
        }
    }
}
