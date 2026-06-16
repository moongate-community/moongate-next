using System.Linq;
using Moongate.Core.Ids;
using Moongate.Server.Data.Internal.Packets;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types.Items;
using Xunit;

namespace Moongate.Tests.Server.World;

public sealed class WornItemLayersTests
{
    [Fact]
    public void VisibleEquipped_ExcludesBackpackAndBank()
    {
        var mobile = new MobileEntity();
        mobile.EquippedItemIds[ItemLayerType.OuterTorso] = new Serial(Serial.ItemOffset + 1);
        mobile.EquippedItemIds[ItemLayerType.Backpack] = new Serial(Serial.ItemOffset + 2);
        mobile.EquippedItemIds[ItemLayerType.Bank] = new Serial(Serial.ItemOffset + 3);
        mobile.EquippedItemIds[ItemLayerType.Helm] = new Serial(Serial.ItemOffset + 4);

        var visible = WornItemLayers.VisibleEquipped(mobile).Select(kv => kv.Key).ToHashSet();

        Assert.Contains(ItemLayerType.OuterTorso, visible);
        Assert.Contains(ItemLayerType.Helm, visible);
        Assert.DoesNotContain(ItemLayerType.Backpack, visible);
        Assert.DoesNotContain(ItemLayerType.Bank, visible);
    }
}
