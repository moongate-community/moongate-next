using Moongate.UO.Data.Types.Items;

namespace Moongate.UO.Data.Animations;

/// <summary>Back-to-front draw priority for compositing a paperdoll (lower draws first).</summary>
public static class PaperdollDrawOrder
{
    /// <summary>Priority for the paperdoll background frame (drawn first).</summary>
    public const int BackgroundPriority = -100;

    /// <summary>Priority for the base body gump (drawn after the background, before equipment).</summary>
    public const int BodyPriority = -50;

    /// <summary>Sentinel for layers that are not drawn on a paperdoll.</summary>
    public const int Skip = int.MaxValue;

    /// <summary>Returns the paperdoll draw priority for an equipment layer, or <see cref="Skip" />.</summary>
    public static int Priority(ItemLayerType layer)
    {
        return layer switch
        {
            ItemLayerType.Cloak => 10,
            ItemLayerType.Shirt => 20,
            ItemLayerType.Pants => 30,
            ItemLayerType.InnerLegs => 40,
            ItemLayerType.Shoes => 50,
            ItemLayerType.InnerTorso => 60,
            ItemLayerType.Arms => 70,
            ItemLayerType.MiddleTorso => 80,
            ItemLayerType.OuterLegs => 90,
            ItemLayerType.Neck => 100,
            ItemLayerType.Waist => 110,
            ItemLayerType.OuterTorso => 120,
            ItemLayerType.Gloves => 130,
            ItemLayerType.Ring => 140,
            ItemLayerType.Talisman => 150,
            ItemLayerType.Bracelet => 160,
            ItemLayerType.Hair => 170,
            ItemLayerType.FacialHair => 180,
            ItemLayerType.Earrings => 190,
            ItemLayerType.Helm => 200,
            ItemLayerType.OneHanded => 210,
            ItemLayerType.TwoHanded => 220,
            _ => Skip
        };
    }
}
