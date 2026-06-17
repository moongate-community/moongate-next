using Moongate.Server.Data.World;

namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
///     Provides access to container defaults and layouts loaded from server asset data.
/// </summary>
public interface IContainerDataService : IDataService
{
    /// <summary>
    ///     Returns all loaded default container definitions.
    /// </summary>
    /// <returns>All default container definitions.</returns>
    IReadOnlyList<ContainerEntry> GetAllContainers();

    /// <summary>
    ///     Returns all loaded container layout definitions.
    /// </summary>
    /// <returns>All container layout definitions.</returns>
    IReadOnlyList<ContainerLayoutEntry> GetAllLayouts();

    /// <summary>
    ///     Replaces all currently loaded default container definitions.
    /// </summary>
    /// <param name="entries">Container definitions.</param>
    void SetContainers(IReadOnlyList<ContainerEntry> entries);

    /// <summary>
    ///     Replaces all currently loaded container layout definitions.
    /// </summary>
    /// <param name="entries">Container layout definitions.</param>
    void SetLayouts(IReadOnlyList<ContainerLayoutEntry> entries);
}
