using Moongate.UO.Data.Types.Items;

namespace Moongate.UO.Data.Animations;

/// <summary>
///     Front-facing draw priority for mobile figure layers (lower draws first/behind). The body sits at
///     <see cref="BodyPriority" />; non-drawable layers return <see cref="Skip" />. Tuned for a catalog
///     thumbnail (single facing); per-direction order is a later refinement.
/// </summary>
public static class EquipmentDrawOrder
{
    public const int Skip = int.MaxValue;

    public const int BodyPriority = 5;

    public static int Priority(ItemLayerType layer)
    {
        return layer switch
        {
            ItemLayerType.Cloak => 0,
            ItemLayerType.InnerLegs => 10,
            ItemLayerType.Pants => 11,
            ItemLayerType.Shoes => 12,
            ItemLayerType.Shirt => 13,
            ItemLayerType.InnerTorso => 14,
            ItemLayerType.Arms => 15,
            ItemLayerType.MiddleTorso => 16,
            ItemLayerType.Gloves => 17,
            ItemLayerType.Neck => 18,
            ItemLayerType.Waist => 19,
            ItemLayerType.OuterLegs => 20,
            ItemLayerType.OuterTorso => 21,
            ItemLayerType.Hair => 25,
            ItemLayerType.FacialHair => 26,
            ItemLayerType.Helm => 30,
            ItemLayerType.OneHanded => 35,
            ItemLayerType.TwoHanded => 36,
            _ => Skip
        };
    }
}
