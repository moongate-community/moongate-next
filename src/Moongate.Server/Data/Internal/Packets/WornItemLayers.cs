using Moongate.Core.Ids;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Server.Data.Internal.Packets;

/// <summary>
/// Selects the equipped layer/serial pairs that should be sent to clients as visible worn items.
/// </summary>
public static class WornItemLayers
{
    public static IEnumerable<KeyValuePair<ItemLayerType, Serial>> VisibleEquipped(MobileEntity mobile)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        return mobile.EquippedItemIds.Where(static kv =>
            kv.Key != ItemLayerType.Backpack && kv.Key != ItemLayerType.Bank);
    }
}
