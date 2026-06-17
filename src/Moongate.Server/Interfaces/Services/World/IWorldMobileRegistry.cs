using Moongate.Core.Ids;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
/// In-memory registry of the live in-world mobiles (mutated in place; not persisted per change).
/// </summary>
public interface IWorldMobileRegistry
{
    /// <summary>Adds or replaces the live mobile keyed by its serial.</summary>
    void Add(MobileEntity mobile);

    /// <summary>Gets the live mobile by serial, or false when absent.</summary>
    bool TryGet(Serial id, out MobileEntity mobile);

    /// <summary>Removes the live mobile; returns false when absent.</summary>
    bool Remove(Serial id);

    /// <summary>Snapshot of all live mobiles.</summary>
    IReadOnlyCollection<MobileEntity> All { get; }
}
