using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.UO.Data.Data.Items;
using Moongate.UO.Data.Interfaces.Entities.Base;
using Moongate.UO.Data.Types.Items;

namespace Moongate.UO.Data.Interfaces.Entities.Items;

/// <summary>
/// Contract for item entities: a world entity with item-specific graphic,
/// stacking, placement (in a container or equipped on a mobile) and, when the
/// item's art is a container, child-item containment.
/// </summary>
public interface IItemEntity : IWorldEntity
{
    /// <summary>Art/graphic identifier of the item.</summary>
    int ItemId { get; set; }

    /// <summary>Stack quantity; 1 for non-stacked items.</summary>
    int Amount { get; set; }

    /// <summary>Whether this item type can stack with others of the same kind.</summary>
    bool IsStackable { get; set; }

    /// <summary>Weight of a single unit of this item.</summary>
    int Weight { get; set; }

    /// <summary>Optional container gump identifier shown when the item is opened.</summary>
    int? GumpId { get; set; }

    /// <summary>Identifier of the script that drives this item's behavior, if any.</summary>
    string ScriptId { get; set; }

    /// <summary>Rarity tier of the item.</summary>
    ItemRarity Rarity { get; set; }

    /// <summary>Minimum account level required to see or interact with the item.</summary>
    UserLevelType Visibility { get; set; }

    /// <summary>Serial of the parent container, or default when the item is not inside one.</summary>
    Serial ParentContainerId { get; set; }

    /// <summary>Position of the item inside its parent container.</summary>
    Point2D ContainerPosition { get; set; }

    /// <summary>Serial of the mobile wearing the item, or default when it is not equipped.</summary>
    Serial EquippedMobileId { get; set; }

    /// <summary>Paperdoll layer the item occupies when equipped, or null when not equipped.</summary>
    ItemLayerType? EquippedLayer { get; set; }

    /// <summary>Persisted serials of the items contained when this item acts as a container.</summary>
    List<Serial> ContainedItemIds { get; set; }

    /// <summary>True when this item's art allows it to hold other items (data-driven from tile data).</summary>
    bool IsContainer { get; }

    /// <summary>True when this item's art is a door (data-driven from tile data).</summary>
    bool IsDoor { get; }

    /// <summary>Typed custom properties keyed by name, for plugin- or script-defined data.</summary>
    Dictionary<string, ItemCustomProperty> CustomProperties { get; set; }

    /// <summary>Effective weight including stack amount and any contained items.</summary>
    int TotalWeight { get; }

    /// <summary>Adds a child item at the given position when this item is a container.</summary>
    void AddItem(IItemEntity item, Point2D position);

    /// <summary>Removes a child item by serial; returns true when one was removed.</summary>
    bool RemoveItem(Serial itemId);
}
