using Moongate.Server.Services.Loot;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Loot;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.Server.Loot;

public sealed class LootTemplateProjectionServiceTests
{
    [Fact]
    public void Project_DirectItem_AddsItemImageAndAmount()
    {
        var service = new LootTemplateProjectionService([Item("gold_coin", 0x0EED, true, "Gold Coin")]);
        var table = Table("orc_common", new LootNode { Item = "gold_coin", Amount = new(20, 90), Chance = 1.0 });

        var detail = service.Project(table);

        var row = Assert.Single(detail.Nodes);
        Assert.Equal("item", row.Kind);
        Assert.Equal("gold_coin", row.ItemTemplateId);
        Assert.Equal("Gold Coin", row.Label);
        Assert.Equal("0x0EED", row.ItemIdHex);
        Assert.Equal("/api/items/0x0EED.png", row.ImageUrl);
        Assert.Equal(20, row.AmountMin);
        Assert.Equal(90, row.AmountMax);
        Assert.Equal(1.0, row.Chance);
        Assert.Single(detail.PreviewItems);
    }

    [Fact]
    public void Project_Category_ExpandsConcreteTaggedItems()
    {
        var service = new LootTemplateProjectionService(
            [
                Item("ruby", 0x0F13, false, "Ruby", "gem"),
                Item("sapphire", 0x0F19, false, "Sapphire", "gem"),
                Item("abstract_gem", 0x0000, false, "Abstract Gem", "gem", true)
            ]
        );
        var table = Table("gems", new LootNode { Category = "gem", Chance = 0.25 });

        var detail = service.Project(table);

        Assert.Equal(3, detail.Nodes.Count);
        Assert.Contains(detail.Nodes, row => row.Kind == "category" && row.Label == "gem");
        Assert.Contains(detail.Nodes, row => row.ItemTemplateId == "ruby");
        Assert.Contains(detail.Nodes, row => row.ItemTemplateId == "sapphire");
        Assert.DoesNotContain(detail.Nodes, row => row.ItemTemplateId == "abstract_gem");

        var candidates = detail.Nodes.Where(static row => row.Kind == "category_candidate").ToArray();
        Assert.Equal(2, candidates.Length);
        Assert.All(candidates, row => Assert.Equal(0.125, row.Chance));
        Assert.All(candidates, row => Assert.Equal(0, row.Weight));
        Assert.Equal(2, detail.PreviewItems.Count);
    }

    private static ItemTemplateDefinition Item(
        string id,
        int itemId,
        bool stackable,
        string name,
        string tag = "",
        bool isAbstract = false
    )
    {
        var template = new ItemTemplateDefinition
        {
            Id = id,
            ItemId = itemId,
            IsStackable = stackable,
            Name = name,
            IsAbstract = isAbstract,
            Rarity = ItemRarity.Common
        };

        if (!string.IsNullOrWhiteSpace(tag))
        {
            template.Tags.Add(tag);
        }

        return template;
    }

    private static LootTableDefinition Table(string id, params LootNode[] nodes)
        => new() { Id = id, Content = [.. nodes] };
}
