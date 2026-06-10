using Moongate.Server.Services.Loot;
using Moongate.UO.Data.Templates.Loot;

namespace Moongate.Tests.UO.Data.Templates.Loot;

public sealed class LootTableDefinitionTests
{
    [Fact]
    public void Deserialize_FixedAmount_MapsMinEqualsMax()
    {
        const string yaml =
            """
            loot_tables:
              - id: t
                content:
                  - item: gold_coin
                    amount: 7
            """;

        var table = LootYaml.Deserializer.Deserialize<LootTableTable>(yaml);

        var amount = table.LootTables[0].Content[0].Amount;
        Assert.NotNull(amount);
        Assert.Equal(7, amount.Min);
        Assert.Equal(7, amount.Max);
    }

    [Fact]
    public void Deserialize_FullSchema_MapsTree()
    {
        const string yaml =
            """
            loot_tables:
              - id: common
                content:
                  - item: gold_coin
                    amount: { min: 1, max: 100 }
                  - pick_one_of:
                      - item: apple
                      - item: bread_loaf
                      - category: armor
                        weight: 3
                  - category: reagent
                    chance: 0.5
                    amount: 2
            """;

        var table = LootYaml.Deserializer.Deserialize<LootTableTable>(yaml);

        var common = Assert.Single(table.LootTables);
        Assert.Equal("common", common.Id);
        Assert.Equal(3, common.Content.Count);

        var gold = common.Content[0];
        Assert.Equal("gold_coin", gold.Item);
        Assert.NotNull(gold.Amount);
        Assert.Equal(1, gold.Amount.Min);
        Assert.Equal(100, gold.Amount.Max);
        Assert.Equal(1.0, gold.Chance);

        var pick = common.Content[1];
        Assert.NotNull(pick.PickOneOf);
        Assert.Equal(3, pick.PickOneOf.Count);
        Assert.Equal("armor", pick.PickOneOf[2].Category);
        Assert.Equal(3, pick.PickOneOf[2].Weight);

        var reagent = common.Content[2];
        Assert.Equal("reagent", reagent.Category);
        Assert.Equal(0.5, reagent.Chance);
        Assert.NotNull(reagent.Amount);
        Assert.Equal(2, reagent.Amount.Min);
        Assert.Equal(2, reagent.Amount.Max);
    }

    [Fact]
    public void Deserialize_NodeDefaults_ChanceOneWeightOneAmountNull()
    {
        const string yaml =
            """
            loot_tables:
              - id: t
                content:
                  - item: gold_coin
            """;

        var node = LootYaml.Deserializer.Deserialize<LootTableTable>(yaml).LootTables[0].Content[0];
        Assert.Equal(1.0, node.Chance);
        Assert.Equal(1, node.Weight);
        Assert.Null(node.Amount);
        Assert.Null(node.Category);
        Assert.Null(node.PickOneOf);
        Assert.Null(node.Group);
    }
}
