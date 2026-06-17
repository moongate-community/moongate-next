using Moongate.Server.Data.World;

namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
///     Provides access to weather definitions loaded from server asset data.
/// </summary>
public interface IWeatherDataService : IDataService
{
    /// <summary>
    ///     Returns all loaded weather entries.
    /// </summary>
    /// <returns>All weather entries.</returns>
    IReadOnlyList<WeatherEntry> GetAllEntries();

    /// <summary>
    ///     Replaces all currently loaded weather entries.
    /// </summary>
    /// <param name="entries">Weather entries.</param>
    void SetEntries(IReadOnlyList<WeatherEntry> entries);
}
