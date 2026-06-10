using Moongate.Server.Services.Loot;
using Moongate.Server.Services.Templates;
using Moongate.Tests.Support;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using ShaiRandom.Generators;

namespace Moongate.Tests.Server.Loot;

public sealed class LootTableBootServiceTests
{
    private static ItemTemplateService Templates()
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange([new ItemTemplateDefinition { Id = "gold_coin", ItemId = 3821, IsStackable = true }]);

        return registry;
    }

    private static LootService NewLootService(ItemTemplateService templates)
        => new(
            templates,
            new Lazy<IItemFactoryService>(static () => throw new NotSupportedException()),
            new MizuchiRandom(1UL, 1UL)
        );

    [Fact]
    public async Task StartAsync_ValidFile_PopulatesService()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("common.yaml", "loot_tables:\n  - id: common\n    content:\n      - item: gold_coin\n");
        var templates = Templates();
        var loot = NewLootService(templates);
        var bootService = new LootTableBootService(new LootTableYamlLoader(dir.Path), loot, templates);

        await bootService.StartAsync(CancellationToken.None);

        Assert.True(loot.Has("common"));
    }

    [Fact]
    public async Task StartAsync_InvalidFile_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("bad.yaml", "loot_tables:\n  - id: t\n    content:\n      - item: does_not_exist\n");
        var templates = Templates();
        var bootService = new LootTableBootService(new LootTableYamlLoader(dir.Path), NewLootService(templates), templates);

        await Assert.ThrowsAsync<InvalidOperationException>(() => bootService.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task StartAsync_MissingDirectory_NoThrowAndNoTables()
    {
        using var dir = new TempTemplateDirectory();
        var templates = Templates();
        var loot = NewLootService(templates);
        var bootService = new LootTableBootService(new LootTableYamlLoader(dir.Path), loot, templates);

        await bootService.StartAsync(CancellationToken.None);

        Assert.False(loot.Has("anything"));
    }
}
