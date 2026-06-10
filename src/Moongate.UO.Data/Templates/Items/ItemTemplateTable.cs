namespace Moongate.UO.Data.Templates.Items;

/// <summary>
/// Root YAML document for item template files: a list under <c>item_templates</c>.
/// </summary>
public sealed class ItemTemplateTable
{
    public List<ItemTemplateDefinition> ItemTemplates { get; set; } = [];
}
