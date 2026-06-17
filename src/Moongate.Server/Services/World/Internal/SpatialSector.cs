using Moongate.Core.Ids;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Server.Services.World.Internal;

/// <summary>
/// Bucket of positioned entities within a single map sector: all mobiles, the player
/// subset, and ground items. Not thread-safe on its own; guarded by the owning index lock.
/// </summary>
internal sealed class SpatialSector
{
    public Dictionary<Serial, MobileEntity> Mobiles { get; } = [];
    public Dictionary<Serial, MobileEntity> Players { get; } = [];
    public Dictionary<Serial, ItemEntity> Items { get; } = [];

    public bool IsEmpty => Mobiles.Count == 0 && Players.Count == 0 && Items.Count == 0;
}
