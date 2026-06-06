using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Server.Services.World;

/// <summary>
/// In-memory store for weather entries loaded at startup.
/// </summary>
public class WeatherDataService : IWeatherDataService
{
    private readonly object _sync = new();
    private List<WeatherEntry> _entries = [];

    public IReadOnlyList<WeatherEntry> GetAllEntries()
    {
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
    }
}
