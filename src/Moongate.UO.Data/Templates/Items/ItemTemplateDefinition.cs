using Moongate.Core.Types;
using Moongate.UO.Data.Types.Items;

namespace Moongate.UO.Data.Templates.Items;

/// <summary>
/// Declarative item template loaded from YAML. The <see cref="BaseItem" />
/// chain is resolved at boot before the template enters the registry, so
/// registered instances are always fully merged.
/// </summary>
public sealed class ItemTemplateDefinition
{
    /// <summary>
    /// Param key reserved for the template's <see cref="IsMovable" /> flag on
    /// created entities: the loader rejects templates declaring it and the
    /// factory writes it into the entity custom properties.
    /// </summary>
    public const string ReservedIsMovableParamKey = "is_movable";

    public string Id { get; set; } = "";

    public string? BaseItem { get; set; }

    public bool IsAbstract { get; set; }

    public string Name { get; set; } = "";

    public string Comment { get; set; } = "";

    public int ItemId { get; set; }

    public int Hue { get; set; }

    public int Weight { get; set; }

    public int Amount { get; set; } = 1;

    public bool IsStackable { get; set; }

    public bool IsMovable { get; set; }

    public int? GumpId { get; set; }

    public ItemLayerType? Layer { get; set; }

    public string ScriptId { get; set; } = "";

    /// <summary>Item rarity; templates default to Common.</summary>
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;

    public ItemTemplateValueDefinition? Value { get; set; }

    public UserLevelType Visibility { get; set; }

    public List<string> Tags { get; set; } = [];

    public Dictionary<string, ItemTemplateParamDefinition> Params { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
