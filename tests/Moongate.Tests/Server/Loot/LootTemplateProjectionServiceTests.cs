using Moongate.Server.Services.Loot;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Loot;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.Server.Loot;

public sealed class LootTemplateProjectionServiceTests
{
    [Fact]
    public void Project_Category_KeepsDefinitionSeparateFromPotentialItems()
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

        var definition = Assert.Single(detail.Nodes);
        Assert.Equal("category", definition.Kind);
        Assert.Equal("gem", definition.Label);
        Assert.Contains(detail.Nodes, row => row.Kind == "category" && row.Label == "gem");
        Assert.DoesNotContain(detail.Nodes, row => row.Kind == "category_candidate");
        Assert.DoesNotContain(detail.Nodes, row => row.ItemTemplateId == "ruby");
        Assert.DoesNotContain(detail.Nodes, row => row.ItemTemplateId == "sapphire");
        Assert.DoesNotContain(detail.Nodes, row => row.ItemTemplateId == "abstract_gem");

        var candidates = detail.PotentialItems.Where(static row => row.Kind == "category_candidate").ToArray();
        Assert.Equal(2, candidates.Length);
        Assert.Contains(candidates, row => row.ItemTemplateId == "ruby");
        Assert.Contains(candidates, row => row.ItemTemplateId == "sapphire");
        Assert.DoesNotContain(candidates, row => row.ItemTemplateId == "abstract_gem");
        Assert.All(candidates, row => Assert.Equal(0.125, row.Chance));
        Assert.All(candidates, row => Assert.Equal(0, row.Weight));
        Assert.All(candidates, row => Assert.Equal("Common", row.Rarity));
        Assert.Equal(2, detail.PreviewItems.Count);
        Assert.Equal(
            detail.PotentialItems.Select(static row => row.ItemTemplateId),
            detail.PreviewItems.Select(static row => row.ItemTemplateId)
        );
    }

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
        Assert.Equal("Common", row.Rarity);
        Assert.Equal("0x0EED", row.ItemIdHex);
        Assert.Equal("/api/items/0x0EED.png", row.ImageUrl);
        Assert.Equal(20, row.AmountMin);
        Assert.Equal(90, row.AmountMax);
        Assert.Equal(1.0, row.Chance);
        Assert.Single(detail.PotentialItems);
        Assert.Single(detail.PreviewItems);
    }

    [Fact]
    public void Project_UsesCurrentItemTemplateSnapshot()
    {
        var templates = new ItemTemplateService();
        templates.ReplaceAll([Item("ruby", 0x0F13, false, "Ruby", "gem")]);
        var service = new LootTemplateProjectionService(templates);
        templates.ReplaceAll([Item("sapphire", 0x0F19, false, "Sapphire", "gem")]);

        var detail = service.Project(Table("gems", new LootNode { Category = "gem" }));

        var candidate = Assert.Single(detail.PotentialItems, static row => row.Kind == "category_candidate");
        Assert.Equal("sapphire", candidate.ItemTemplateId);
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
