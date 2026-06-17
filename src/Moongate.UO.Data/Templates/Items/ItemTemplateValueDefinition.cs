using Moongate.UO.Data.Types.Items;
using YamlDotNet.Serialization;

namespace Moongate.UO.Data.Templates.Items;

public sealed class ItemTemplateValueDefinition
{
    public int Buy { get; set; }

    public int? Sell { get; set; }

    [YamlIgnore] public int BaseSell => Sell ?? Buy / 2;

    public ItemTemplateValueDefinition Clone()
    {
        return new ItemTemplateValueDefinition
        {
            Buy = Buy,
            Sell = Sell
        };
    }

    public int EffectiveBuy(ItemRarity rarity)
    {
        return ApplyMultiplier(Buy, RarityMultiplier(rarity));
    }

    public int EffectiveSell(ItemRarity rarity)
    {
        return ApplyMultiplier(BaseSell, RarityMultiplier(rarity));
    }

    public decimal RarityMultiplier(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Uncommon => 1.25m,
            ItemRarity.Rare => 1.5m,
            ItemRarity.Epic => 2.0m,
            ItemRarity.Legendary => 3.0m,
            _ => 1.0m
        };
    }

    private static int ApplyMultiplier(int value, decimal multiplier)
    {
        return (int)Math.Round(value * multiplier, MidpointRounding.AwayFromZero);
    }
}
