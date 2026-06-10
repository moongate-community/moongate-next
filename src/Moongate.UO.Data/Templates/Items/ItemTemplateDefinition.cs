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
    public string Id { get; set; } = "";

    public string? BaseItem { get; set; }

    public bool IsAbstract { get; set; }

    public string Name { get; set; } = "";

    public int ItemId { get; set; }

    public int Hue { get; set; }

    public int Weight { get; set; }

    public int Amount { get; set; } = 1;

    public bool IsStackable { get; set; }

    public bool IsMovable { get; set; }

    public int? GumpId { get; set; }

    public ItemLayerType? Layer { get; set; }

    public string ScriptId { get; set; } = "";

    public ItemRarity Rarity { get; set; } = ItemRarity.None;

    public UserLevelType Visibility { get; set; }

    public List<string> Tags { get; set; } = [];

    public Dictionary<string, ItemTemplateParamDefinition> Params { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
