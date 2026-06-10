using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.Server.Data.Templates;

public sealed record ItemTemplateParamSummary(string Key, string Type, string Value);

public sealed record ItemTemplateDetail(
    string Id,
    string Name,
    int ItemId,
    string ItemIdHex,
    string ImageUrl,
    string Rarity,
    string? Layer,
    IReadOnlyList<string> Tags,
    bool IsAbstract,
    HueSummary Hue,
    string Comment,
    string? BaseItem,
    string ScriptId,
    string Visibility,
    int Amount,
    int Weight,
    bool IsStackable,
    bool IsMovable,
    int? GumpId,
    IReadOnlyList<ItemTemplateParamSummary> Params
)
{
    public static ItemTemplateDetail FromDefinition(ItemTemplateDefinition template, IHueStore hues)
    {
        var summary = ItemTemplateSummary.FromDefinition(template, hues);
        var parameters = template.Params
                                 .OrderBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                                 .Select(static pair => new ItemTemplateParamSummary(
                                             pair.Key,
                                             pair.Value.Type.ToString(),
                                             pair.Value.Value
                                         ))
                                 .ToArray();

        return new(
            summary.Id,
            summary.Name,
            summary.ItemId,
            summary.ItemIdHex,
            summary.ImageUrl,
            summary.Rarity,
            summary.Layer,
            summary.Tags,
            summary.IsAbstract,
            summary.Hue,
            template.Comment,
            template.BaseItem,
            template.ScriptId,
            template.Visibility.ToString(),
            template.Amount,
            template.Weight,
            template.IsStackable,
            template.IsMovable,
            template.GumpId,
            parameters
        );
    }
}
