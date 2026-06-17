namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
///     Provides common loading state and reload operations for data services.
/// </summary>
public interface IDataService
{
    /// <summary>
    ///     Gets whether this data service loads its data on first use.
    /// </summary>
    bool IsLazy { get; }

    /// <summary>
    ///     Gets whether this data service has already loaded or received data.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    ///     Loads the backing data if it has not already been loaded.
    /// </summary>
    void EnsureLoaded();

    /// <summary>
    ///     Reloads the backing data even when it has already been loaded.
    /// </summary>
    void Reload();
}
