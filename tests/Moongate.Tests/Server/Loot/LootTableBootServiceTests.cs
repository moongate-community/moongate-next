using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Server.Services.Loot;
using Moongate.Server.Services.Templates;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using ShaiRandom.Generators;

namespace Moongate.Tests.Server.Loot;

public sealed class LootTableBootServiceTests
{
    private sealed class FakeItemService : IItemService
    {
        private readonly HashSet<int> _containerItemIds;

        public FakeItemService(params int[] containerItemIds)
        {
            _containerItemIds = [..containerItemIds];
        }

        public ValueTask<bool> AddItemAsync(
            ItemEntity container,
            ItemEntity child,
            Point2D position,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<ItemEntity> CreateAsync(ItemEntity item, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<ItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public bool IsContainer(ItemEntity item)
            => IsContainer(item.ItemId);

        public bool IsContainer(int itemId)
            => _containerItemIds.Contains(itemId);

        public bool IsDoor(ItemEntity item)
            => throw new NotSupportedException();

        public bool IsDoor(int itemId)
            => throw new NotSupportedException();

        public ValueTask<bool> RemoveItemAsync(
            ItemEntity container,
            Serial itemId,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();

        public ValueTask<int> TotalWeightAsync(ItemEntity item, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    [Fact]
    public async Task StartAsync_InvalidFile_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("bad.yaml", "loot_tables:\n  - id: t\n    content:\n      - item: does_not_exist\n");
        var templates = Templates();
        var bootService = new LootTableBootService(
            new(dir.Path),
            NewLootService(templates),
            templates,
            new(),
            new ThrowingItemService()
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => bootService.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task StartAsync_ItemTemplateContentsReferenceUnknownLoot_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("common.yaml", "loot_tables:\n  - id: common\n    content:\n      - item: gold_coin\n");
        var templates = Templates(
            new ItemTemplateDefinition
            {
                Id = "wooden_chest",
                ItemId = 3651,
                Contents = new()
                {
                    LootTemplate = "missing",
                    RefillEvery = TimeSpan.FromHours(6)
                }
            }
        );
        var loot = NewLootService(templates);
        var bootService = new LootTableBootService(new(dir.Path), loot, templates, new(), new FakeItemService(3651));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                            () => bootService.StartAsync(CancellationToken.None)
                        );

        Assert.Contains("wooden_chest", exception.Message);
        Assert.Contains("missing", exception.Message);
    }

    [Fact]
    public async Task StartAsync_MissingDirectory_NoThrowAndNoTables()
    {
        using var dir = new TempTemplateDirectory();
        var templates = Templates();
        var loot = NewLootService(templates);
        var bootService = new LootTableBootService(new(dir.Path), loot, templates, new(), new ThrowingItemService());

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
        var bootService = new LootTableBootService(new(dir.Path), loot, templates, store, new ThrowingItemService());

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

    private static ItemTemplateService Templates(params ItemTemplateDefinition[] additionalTemplates)
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange([new() { Id = "gold_coin", ItemId = 3821, IsStackable = true }, ..additionalTemplates]);

        return registry;
    }
}
