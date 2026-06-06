using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World.Internal;
using Moongate.Server.Services.WorldData;

namespace Moongate.Server.Services.World;

/// <summary>
/// Lazy in-memory store for weather entries.
/// </summary>
public class WeatherDataService : LazyDataService, IWeatherDataService
{
    private readonly ServerAssetDataLoader? _loader;
    private readonly Lock _sync = new();
    private List<WeatherEntry> _entries = [];

    public WeatherDataService()
    {
    }

    public WeatherDataService(ServerAssetDataLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        _loader = loader;
    }

    public IReadOnlyList<WeatherEntry> GetAllEntries()
    {
        EnsureLoaded();

        lock (_sync)
        {
            return [.. _entries];
        }
    }

    public void SetEntries(IReadOnlyList<WeatherEntry> entries)
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
        _loader?.LoadWeather(this);
    }
}
