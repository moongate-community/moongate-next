using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Server.Services.World;

/// <summary>
/// In-memory store for sign entries loaded at startup.
/// </summary>
public class SignDataService : ISignDataService
{
    private readonly object _sync = new();
    private List<SignEntry> _entries = [];
    private Dictionary<int, List<SignEntry>> _entriesByMap = [];

    public IReadOnlyList<SignEntry> GetAllEntries()
    {
        lock (_sync)
        {
            return [.. _entries];
        }
    }

    public IReadOnlyList<SignEntry> GetEntriesByMap(int mapId)
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

    public void SetEntries(IReadOnlyList<SignEntry> entries)
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
