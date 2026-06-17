using Moongate.UO.Data.Animations;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.UO.Data.Animations;

public sealed class EquipmentDrawOrderTests
{
    [Theory]
    [InlineData(ItemLayerType.Ring)]
    [InlineData(ItemLayerType.Talisman)]
    [InlineData(ItemLayerType.Bracelet)]
    [InlineData(ItemLayerType.Earrings)]
    [InlineData(ItemLayerType.Backpack)]
    [InlineData(ItemLayerType.Bank)]
    [InlineData(ItemLayerType.Mount)]
    [InlineData(ItemLayerType.Invalid)]
    public void Priority_NonDrawableLayers_ReturnSkip(ItemLayerType layer)
    {
        Assert.Equal(EquipmentDrawOrder.Skip, EquipmentDrawOrder.Priority(layer));
    }

    [Fact]
    public void Priority_OrdersFromInnerToOuterToWeapons()
    {
        // body is below clothes; inner < outer < hair < helm < weapons
        Assert.True(EquipmentDrawOrder.BodyPriority < EquipmentDrawOrder.Priority(ItemLayerType.Shirt));
        Assert.True(
            EquipmentDrawOrder.Priority(ItemLayerType.Pants) < EquipmentDrawOrder.Priority(ItemLayerType.OuterTorso)
        );
        Assert.True(EquipmentDrawOrder.Priority(ItemLayerType.OuterTorso) < EquipmentDrawOrder.Priority(ItemLayerType.Hair));
        Assert.True(EquipmentDrawOrder.Priority(ItemLayerType.Hair) < EquipmentDrawOrder.Priority(ItemLayerType.Helm));
        Assert.True(EquipmentDrawOrder.Priority(ItemLayerType.Helm) < EquipmentDrawOrder.Priority(ItemLayerType.OneHanded));
    }
}
