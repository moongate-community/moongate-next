using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Server.Services.World;

/// <summary>
/// In-memory store for spawn definitions loaded at startup.
/// </summary>
public class SpawnsDataService : ISpawnsDataService
{
    private readonly object _sync = new();
    private List<SpawnDefinitionEntry> _entries = [];
    private Dictionary<int, List<SpawnDefinitionEntry>> _entriesByMap = [];

    public IReadOnlyList<SpawnDefinitionEntry> GetAllEntries()
    {
        lock (_sync)
        {
            return [.. _entries];
        }
    }

    public IReadOnlyList<SpawnDefinitionEntry> GetEntriesByMap(int mapId)
    {
        lock (_sync)
        {
            if (!_entriesByMap.TryGetValue(mapId, out var entries))
            {
                return [];
            }

            return [.. entries];
        }
    }

    public void SetEntries(IReadOnlyList<SpawnDefinitionEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var snapshot = entries.ToList();

        lock (_sync)
        {
            _entries = snapshot;
            _entriesByMap = snapshot
                .GroupBy(static entry => entry.MapId)
                .ToDictionary(
                    static grouping => grouping.Key,
                    static grouping => grouping.ToList()
                );
        }
    }
}
