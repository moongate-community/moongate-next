using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Server.Data.Templates;

public sealed record ItemTemplateValueSummary(
    int Buy,
    int Sell,
    decimal RarityMultiplier,
    int EffectiveBuy,
    int EffectiveSell
)
{
    public static ItemTemplateValueSummary FromDefinition(ItemTemplateValueDefinition value, ItemRarity rarity)
    {
        return new ItemTemplateValueSummary(
            value.Buy,
            value.BaseSell,
            value.RarityMultiplier(rarity),
            value.EffectiveBuy(rarity),
            value.EffectiveSell(rarity)
        );
    }
}
