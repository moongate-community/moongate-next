using Moongate.UO.Data.Animations;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.UO.Data.Animations;

public sealed class PaperdollDrawOrderTests
{
    [Fact]
    public void Cloak_DrawsBehind_OuterTorso()
        => Assert.True(PaperdollDrawOrder.Priority(ItemLayerType.Cloak) < PaperdollDrawOrder.Priority(ItemLayerType.OuterTorso));

    [Fact]
    public void Helm_DrawsInFrontOf_Hair()
        => Assert.True(PaperdollDrawOrder.Priority(ItemLayerType.Helm) > PaperdollDrawOrder.Priority(ItemLayerType.Hair));

    [Fact]
    public void NonPaperdollLayer_IsSkipped()
        => Assert.Equal(PaperdollDrawOrder.Skip, PaperdollDrawOrder.Priority(ItemLayerType.Backpack));

    [Fact]
    public void BodyAndBackground_SortBeforeEquipment()
    {
        Assert.True(PaperdollDrawOrder.BackgroundPriority < PaperdollDrawOrder.BodyPriority);
        Assert.True(PaperdollDrawOrder.BodyPriority < PaperdollDrawOrder.Priority(ItemLayerType.Shirt));
    }
}
