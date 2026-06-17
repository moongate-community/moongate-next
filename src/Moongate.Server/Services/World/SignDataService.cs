using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World.Internal;
using Moongate.Server.Services.WorldData;

namespace Moongate.Server.Services.World;

/// <summary>
///     Lazy in-memory store for sign entries.
/// </summary>
public class SignDataService : LazyDataService, ISignDataService
{
    private readonly ServerAssetDataLoader? _loader;
    private readonly Lock _sync = new();
    private List<SignEntry> _entries = [];
    private Dictionary<int, List<SignEntry>> _entriesByMap = [];

    public SignDataService()
    {
    }

    public SignDataService(ServerAssetDataLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        _loader = loader;
    }

    public IReadOnlyList<SignEntry> GetAllEntries()
    {
        EnsureLoaded();

        lock (_sync)
        {
            return [.. _entries];
        }
    }

    public IReadOnlyList<SignEntry> GetEntriesByMap(int mapId)
    {
        EnsureLoaded();

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

        MarkLoaded();
    }

    protected override void LoadCore()
    {
        _loader?.LoadSigns(this);
    }
}
