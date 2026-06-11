using Moongate.UO.Data.Templates.Items;

namespace Moongate.Server.Data.Templates;

public sealed record ItemTemplateGraphicVariantSummary(
    int ItemId,
    string ItemIdHex,
    string ImageUrl
)
{
    public static ItemTemplateGraphicVariantSummary FromDefinition(ItemTemplateGraphicVariantDefinition variant)
    {
        ArgumentNullException.ThrowIfNull(variant);

        var itemIdHex = ItemTemplateSummary.FormatItemId(variant.ItemId);

        return new(
            variant.ItemId,
            itemIdHex,
            $"/api/items/{itemIdHex}.png"
        );
    }
}
