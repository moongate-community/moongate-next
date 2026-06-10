using Moongate.Server.Services.Loot;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Loot;

namespace Moongate.Tests.Server.Loot;

public sealed class LootTableValidatorTests
{
    private static ItemTemplateService Templates()
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange(
            [
                new ItemTemplateDefinition { Id = "gold_coin", ItemId = 3821, IsStackable = true, Tags = ["currency"] },
                new ItemTemplateDefinition { Id = "apple", ItemId = 2512, Tags = ["food"] },
                new ItemTemplateDefinition { Id = "leather_cap", ItemId = 7609, Tags = ["armor"] },
                new ItemTemplateDefinition { Id = "base_armor", ItemId = 0, IsAbstract = true, Tags = ["armor"] }
            ]
        );

        return registry;
    }

    private static LootTableDefinition Table(string id, params LootNode[] content)
        => new() { Id = id, Content = [.. content] };

    private static LootNode Item(string template, LootAmount? amount = null, double chance = 1.0)
        => new() { Item = template, Amount = amount, Chance = chance };

    [Fact]
    public void Validate_Valid_DoesNotThrow()
    {
        var tables = new List<LootTableDefinition>
        {
            Table(
                "common",
                Item("gold_coin", new LootAmount(1, 100)),
                new LootNode { PickOneOf = [Item("apple"), new LootNode { Category = "armor", Weight = 2 }] },
                new LootNode { Category = "food", Chance = 0.5 }
            )
        };

        LootTableValidator.Validate(tables, Templates());
    }

    [Fact]
    public void Validate_DuplicateId_Throws()
    {
        var tables = new List<LootTableDefinition> { Table("dup", Item("apple")), Table("DUP", Item("gold_coin")) };

        var exception = Assert.Throws<InvalidOperationException>(() => LootTableValidator.Validate(tables, Templates()));
        Assert.Contains("DUP", exception.Message);
    }

    [Fact]
    public void Validate_UnknownItem_Throws()
    {
        var tables = new List<LootTableDefinition> { Table("t", Item("does_not_exist")) };

        var exception = Assert.Throws<InvalidOperationException>(() => LootTableValidator.Validate(tables, Templates()));
        Assert.Contains("does_not_exist", exception.Message);
    }

    [Fact]
    public void Validate_AbstractItem_Throws()
    {
        var tables = new List<LootTableDefinition> { Table("t", Item("base_armor")) };

        var exception = Assert.Throws<InvalidOperationException>(() => LootTableValidator.Validate(tables, Templates()));
        Assert.Contains("abstract", exception.Message);
    }

    [Fact]
    public void Validate_EmptyCategory_Throws()
    {
        var tables = new List<LootTableDefinition> { Table("t", new LootNode { Category = "nonexistent_tag" }) };

        var exception = Assert.Throws<InvalidOperationException>(() => LootTableValidator.Validate(tables, Templates()));
        Assert.Contains("nonexistent_tag", exception.Message);
    }

    [Fact]
    public void Validate_CategoryMatchingOnlyAbstract_Throws()
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange([new ItemTemplateDefinition { Id = "base_only", IsAbstract = true, Tags = ["ghost"] }]);
        var tables = new List<LootTableDefinition> { Table("t", new LootNode { Category = "ghost" }) };

        Assert.Throws<InvalidOperationException>(() => LootTableValidator.Validate(tables, registry));
    }

    [Fact]
    public void Validate_NodeWithNoType_Throws()
    {
        var tables = new List<LootTableDefinition> { Table("t", new LootNode { Chance = 0.5 }) };

        Assert.Throws<InvalidOperationException>(() => LootTableValidator.Validate(tables, Templates()));
    }

    [Fact]
    public void Validate_NodeWithTwoTypes_Throws()
    {
        var tables = new List<LootTableDefinition> { Table("t", new LootNode { Item = "apple", Category = "food" }) };

        Assert.Throws<InvalidOperationException>(() => LootTableValidator.Validate(tables, Templates()));
    }

    [Fact]
    public void Validate_EmptyPickOneOf_Throws()
    {
        var tables = new List<LootTableDefinition> { Table("t", new LootNode { PickOneOf = [] }) };

        Assert.Throws<InvalidOperationException>(() => LootTableValidator.Validate(tables, Templates()));
    }

    [Fact]
    public void Validate_ChanceOutOfRange_Throws()
    {
        var tables = new List<LootTableDefinition> { Table("t", Item("apple", chance: 1.5)) };

        Assert.Throws<InvalidOperationException>(() => LootTableValidator.Validate(tables, Templates()));
    }

    [Fact]
    public void Validate_AmountMinGreaterThanMax_Throws()
    {
        var tables = new List<LootTableDefinition> { Table("t", Item("gold_coin", new LootAmount(5, 1))) };

        Assert.Throws<InvalidOperationException>(() => LootTableValidator.Validate(tables, Templates()));
    }

    [Fact]
    public void Validate_NegativeAmount_Throws()
    {
        var tables = new List<LootTableDefinition> { Table("t", Item("gold_coin", new LootAmount(-1, 5))) };

        Assert.Throws<InvalidOperationException>(() => LootTableValidator.Validate(tables, Templates()));
    }

    [Fact]
    public void Validate_WeightBelowOne_Throws()
    {
        var pick = new LootNode { PickOneOf = [new LootNode { Item = "apple", Weight = 0 }] };
        var tables = new List<LootTableDefinition> { Table("t", pick) };

        Assert.Throws<InvalidOperationException>(() => LootTableValidator.Validate(tables, Templates()));
    }

    [Fact]
    public void Validate_EmptyId_Throws()
    {
        var tables = new List<LootTableDefinition> { Table("", Item("apple")) };

        Assert.Throws<InvalidOperationException>(() => LootTableValidator.Validate(tables, Templates()));
    }
}
