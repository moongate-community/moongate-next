using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.UO.Data.Data.Items;
using Moongate.UO.Data.Interfaces.Entities.Items;
using Moongate.UO.Data.Types.Items;

namespace Moongate.UO.Data.Entities.Items;

/// <summary>
/// Concrete persisted item entity. Holds item state only; derived facts
/// (IsContainer/IsDoor, total weight) and containment behavior are provided by
/// the item/container service layer. Serialized via contractless MessagePack.
/// </summary>
public sealed class ItemEntity : IItemEntity
{
    public Serial Id { get; set; }

    public string? Name { get; set; }

    public DirectionType Direction { get; set; }

    public Point3D Location { get; set; }

    public Hue Hue { get; set; }

    public int MapId { get; set; }

    public int ItemId { get; set; }

    public int Amount { get; set; } = 1;

    public bool IsStackable { get; set; }

    public int Weight { get; set; }

    public int? GumpId { get; set; }

    public string ScriptId { get; set; } = "";

    public ItemRarity Rarity { get; set; }

    public UserLevelType Visibility { get; set; }

    public Serial ParentContainerId { get; set; }

    public Point2D ContainerPosition { get; set; }

    public Serial EquippedMobileId { get; set; }

    public ItemLayerType? EquippedLayer { get; set; }

    public List<Serial> ContainedItemIds { get; set; } = [];

    public Dictionary<string, ItemCustomProperty> CustomProperties { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
