using Moongate.Core.Types;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Server.Data.Templates;

public sealed class ItemTemplateEditRequest
{
    public string Id { get; set; } = "";

    public string? BaseItem { get; set; }

    public bool IsAbstract { get; set; }

    public string Name { get; set; } = "";

    public string Comment { get; set; } = "";

    public int ItemId { get; set; }

    public List<ItemTemplateGraphicVariantDefinition> GraphicVariants { get; set; } = [];

    public int Hue { get; set; }

    public int Weight { get; set; }

    public int Amount { get; set; } = 1;

    public bool IsStackable { get; set; }

    public bool IsMovable { get; set; }

    public int? GumpId { get; set; }

    public ItemLayerType? Layer { get; set; }

    public string ScriptId { get; set; } = "";

    public ItemRarity Rarity { get; set; } = ItemRarity.Common;

    public ItemTemplateValueDefinition? Value { get; set; }

    public ItemTemplateContentsDefinition? Contents { get; set; }

    public UserLevelType Visibility { get; set; }

    public List<string> Tags { get; set; } = [];

    public Dictionary<string, ItemTemplateParamDefinition> Params { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
