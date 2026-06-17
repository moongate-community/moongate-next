using Moongate.UO.Data.Data.Multi;

namespace Moongate.UO.Data.Interfaces.Multi;

/// <summary>
///     Provides access to the parsed UO multi component lists.
/// </summary>
public interface IMultiDataStore
{
    /// <summary>Number of loaded multis.</summary>
    int Count { get; }

    /// <summary>
    ///     Returns the component list for <paramref name="multiId" /> (masked to 14 bits), or
    ///     <see cref="MultiComponentList.Empty" /> when none is loaded.
    /// </summary>
    /// <param name="multiId">Multi id.</param>
    MultiComponentList GetComponents(int multiId);
}
