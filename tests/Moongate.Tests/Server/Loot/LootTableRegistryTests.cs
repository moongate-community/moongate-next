using Moongate.Server.Services.Loot;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Loot;

namespace Moongate.Tests.Server.Loot;

public sealed class LootTableRegistryTests
{
    [Fact]
    public void TryGet_IsCaseInsensitive()
    {
        var registry = new LootTableRegistry([Table("Common")], []);

        Assert.True(registry.TryGet("common", out var table));
        Assert.Equal("Common", table.Id);
    }

    [Fact]
    public void TryGet_Unknown_ReturnsFalse()
    {
        var registry = new LootTableRegistry([], []);

        Assert.False(registry.TryGet("missing", out _));
    }

    [Fact]
    public void TryGetByTag_IsCaseInsensitive()
    {
        var registry = new LootTableRegistry([], [Tmpl("apple", false, "Food")]);

        Assert.True(registry.TryGetByTag("food", out var matches));
        Assert.Single(matches);
    }

    [Fact]
    public void TryGetByTag_ReturnsOnlyConcreteTemplates()
    {
        var templates = new[] { Tmpl("leather_cap", false, "armor"), Tmpl("base_armor", true, "armor") };
        var registry = new LootTableRegistry([], templates);

        Assert.True(registry.TryGetByTag("armor", out var matches));
        Assert.Single(matches);
        Assert.Equal("leather_cap", matches[0].Id);
    }

    [Fact]
    public void TryGetByTag_Unknown_ReturnsFalse()
    {
        var registry = new LootTableRegistry([], []);

        Assert.False(registry.TryGetByTag("nope", out _));
    }

    private static LootTableDefinition Table(string id)
        => new() { Id = id, Content = [new() { Item = "apple" }] };

    private static ItemTemplateDefinition Tmpl(string id, bool isAbstract, params string[] tags)
        => new() { Id = id, IsAbstract = isAbstract, Tags = [.. tags] };
}
