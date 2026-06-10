using System.Globalization;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Server.Data.Templates;

public sealed record ItemTemplateSummary(
    string Id,
    string Name,
    int ItemId,
    string ItemIdHex,
    string ImageUrl,
    string Rarity,
    string? Layer,
    IReadOnlyList<string> Tags,
    bool IsAbstract,
    ItemTemplateValueSummary? Value,
    HueSummary Hue
)
{
    public static ItemTemplateSummary FromDefinition(ItemTemplateDefinition template, IHueStore hues)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(hues);

        var itemIdHex = FormatItemId(template.ItemId);

        return new(
            template.Id,
            template.Name,
            template.ItemId,
            itemIdHex,
            $"/api/items/{itemIdHex}.png",
            template.Rarity.ToString(),
            template.Layer.HasValue ? FormatLayer(template.Layer.Value) : null,
            [..template.Tags],
            template.IsAbstract,
            template.Value is null ? null : ItemTemplateValueSummary.FromDefinition(template.Value, template.Rarity),
            HueSummary.FromValue(template.Hue, hues)
        );
    }

    public static string FormatItemId(int itemId)
        => $"0x{itemId.ToString("X4", CultureInfo.InvariantCulture)}";

    private static string FormatLayer(ItemLayerType layer)
        => layer switch
        {
            ItemLayerType.OneHanded => nameof(ItemLayerType.OneHanded),
            ItemLayerType.InnerLegs => nameof(ItemLayerType.InnerLegs),
            ItemLayerType.Bank      => nameof(ItemLayerType.Bank),
            _                       => layer.ToString()
        };
}
