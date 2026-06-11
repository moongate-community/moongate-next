using Moongate.Server.Services.Loot;
using Moongate.Server.Services.Templates;
using Moongate.Tests.Support;
using ShaiRandom.Generators;

namespace Moongate.Tests.Server.Loot;

public sealed class LootTableBootServiceTests
{
    [Fact]
    public async Task StartAsync_InvalidFile_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("bad.yaml", "loot_tables:\n  - id: t\n    content:\n      - item: does_not_exist\n");
        var templates = Templates();
        var bootService = new LootTableBootService(new(dir.Path), NewLootService(templates), templates, new());

        await Assert.ThrowsAsync<InvalidOperationException>(() => bootService.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task StartAsync_MissingDirectory_NoThrowAndNoTables()
    {
        using var dir = new TempTemplateDirectory();
        var templates = Templates();
        var loot = NewLootService(templates);
        var bootService = new LootTableBootService(new(dir.Path), loot, templates, new());

        await bootService.StartAsync(CancellationToken.None);

        Assert.False(loot.Has("anything"));
    }

    [Fact]
    public async Task StartAsync_ValidFile_PopulatesService()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("common.yaml", "loot_tables:\n  - id: common\n    content:\n      - item: gold_coin\n");
        var templates = Templates();
        var loot = NewLootService(templates);
        var store = new LootTableRegistryStore();
        var bootService = new LootTableBootService(new(dir.Path), loot, templates, store);

        await bootService.StartAsync(CancellationToken.None);

        Assert.True(loot.Has("common"));
        Assert.True(store.IsReady);
        Assert.True(store.Registry.TryGet("common", out _));
    }

    private static LootService NewLootService(ItemTemplateService templates)
        => new(
            templates,
            new(static () => throw new NotSupportedException()),
            new MizuchiRandom(1UL, 1UL)
        );

    private static ItemTemplateService Templates()
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange([new() { Id = "gold_coin", ItemId = 3821, IsStackable = true }]);

        return registry;
    }
}
