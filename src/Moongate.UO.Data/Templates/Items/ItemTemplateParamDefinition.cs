using Moongate.UO.Data.Types.Items;

namespace Moongate.UO.Data.Templates.Items;

/// <summary>
///     A typed extension value attached to an item template; copied onto created
///     entities as a custom property.
/// </summary>
public sealed class ItemTemplateParamDefinition
{
    public ItemTemplateParamType Type { get; set; } = ItemTemplateParamType.String;

    public string Value { get; set; } = "";
}
