using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Server.Services.World;

/// <summary>
/// In-memory store for region entries loaded at startup.
/// </summary>
public class RegionDataService : IRegionDataService
{
    private readonly object _sync = new();
    private List<RegionEntry> _entries = [];

    public IReadOnlyList<RegionEntry> GetAllEntries()
    {
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
    }
}
