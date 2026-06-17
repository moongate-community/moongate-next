using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.Items;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Data;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Properties;

namespace Moongate.Tests.Server.Items;

public sealed class ContainerContentServiceTests
{
    [Fact]
    public async Task EnsureContentsAsync_EmptyGeneratedContainerBeforeRefillDue_DoesNotRefill()
    {
        var service = NewService([Item(Serial.ItemOffset + 2, 3821)], out var items, out var loot);
        var container = Container();
        container.CustomProperties[ItemTemplateDefinitionKeys.ContentsGeneratedAt] = IntegerProperty(100);
        container.CustomProperties[ItemTemplateDefinitionKeys.ContentsNextRefillAt] =
            IntegerProperty(DateTimeOffset.MaxValue.ToUnixTimeMilliseconds());

        await service.EnsureContentsAsync(container);

        Assert.Equal(0, loot.GenerateCalls);
        Assert.Empty(items.Added);
    }

    [Fact]
    public async Task EnsureContentsAsync_EmptyGeneratedContainerRefillsWhenDue()
    {
        var child = Item(Serial.ItemOffset + 2, 3821);
        var service = NewService([child], out var items, out var loot);
        var container = Container();
        container.CustomProperties[ItemTemplateDefinitionKeys.ContentsGeneratedAt] = IntegerProperty(100);
        container.CustomProperties[ItemTemplateDefinitionKeys.ContentsNextRefillAt] = IntegerProperty(100);

        await service.EnsureContentsAsync(container);

        Assert.Equal(1, loot.GenerateCalls);
        Assert.Single(items.Added);
        Assert.True(container.CustomProperties[ItemTemplateDefinitionKeys.ContentsNextRefillAt].IntegerValue > 100);
    }

    [Fact]
    public async Task EnsureContentsAsync_MissingTemplateId_DoesNotThrow()
    {
        var service = NewService([Item(Serial.ItemOffset + 2, 3821)], out var items, out var loot);
        var container = Container();
        container.CustomProperties.Remove(ItemTemplateDefinitionKeys.TemplateId);

        await service.EnsureContentsAsync(container);

        Assert.Equal(0, loot.GenerateCalls);
        Assert.Empty(items.Added);
    }

    [Fact]
    public async Task EnsureContentsAsync_NonEmptyContainer_DoesNotRefill()
    {
        var service = NewService([Item(Serial.ItemOffset + 2, 3821)], out var items, out var loot);
        var container = Container();
        container.ContainedItemIds.Add(new Serial(Serial.ItemOffset + 10));

        await service.EnsureContentsAsync(container);

        Assert.Equal(0, loot.GenerateCalls);
        Assert.Empty(items.Added);
    }

    [Fact]
    public async Task EnsureContentsAsync_NonWorldOwnedContainer_DoesNotGenerate()
    {
        var service = NewService([Item(Serial.ItemOffset + 2, 3821)], out var items, out var loot);
        var container = Container();
        container.ParentContainerId = new Serial(Serial.ItemOffset + 9);

        await service.EnsureContentsAsync(container);

        Assert.Equal(0, loot.GenerateCalls);
        Assert.Empty(items.Added);
    }

    [Fact]
    public async Task EnsureContentsAsync_WorldOwnedEmptyContainer_GeneratesContents()
    {
        var child = Item(Serial.ItemOffset + 2, 3821);
        var service = NewService([child], out var items, out var loot);
        var container = Container();

        await service.EnsureContentsAsync(container);

        Assert.Equal(1, loot.GenerateCalls);
        var added = Assert.Single(items.Added);
        Assert.Same(container, added.Container);
        Assert.Same(child, added.Child);
        Assert.Equal(new Point2D(44, 65), added.Position);
        Assert.Contains(child.Id, container.ContainedItemIds);
        Assert.Equal(container.Id, child.ParentContainerId);
        Assert.True(container.CustomProperties.ContainsKey(ItemTemplateDefinitionKeys.ContentsGeneratedAt));
        Assert.True(container.CustomProperties.ContainsKey(ItemTemplateDefinitionKeys.ContentsNextRefillAt));
    }

    private static ItemEntity Container()
    {
        return new ItemEntity
        {
            Id = new Serial(Serial.ItemOffset + 1),
            ItemId = 3651,
            GumpId = 60,
            CustomProperties =
            {
                [ItemTemplateDefinitionKeys.TemplateId] = StringProperty("wooden_chest")
            }
        };
    }

    private static CustomProperty IntegerProperty(long value)
    {
        return new CustomProperty
        {
            Type = CustomPropertyType.Integer,
            IntegerValue = value
        };
    }

    private static ItemEntity Item(uint serial, int itemId)
    {
        return new ItemEntity
        {
            Id = new Serial(serial),
            ItemId = itemId
        };
    }

    private static ContainerContentService NewService(
        IReadOnlyList<ItemEntity> generatedItems,
        out FakeItemService items,
        out FakeLootService loot
    )
    {
        var templates = new ItemTemplateService();
        templates.UpsertRange(
            [
                new ItemTemplateDefinition
                {
                    Id = "wooden_chest",
                    ItemId = 3651,
                    Contents = new ItemTemplateContentsDefinition
                    {
                        LootTemplate = "common",
                        RefillEvery = TimeSpan.FromHours(6)
                    }
                }
            ]
        );
        items = new FakeItemService(3651);
        loot = new FakeLootService(generatedItems);
        var containers = new FakeContainerDataService(new ContainerLayoutEntry(60, [44, 65, 142, 94], 0, [3651]));

        return new ContainerContentService(templates, items, loot, containers);
    }

    private static CustomProperty StringProperty(string value)
    {
        return new CustomProperty
        {
            Type = CustomPropertyType.String,
            StringValue = value
        };
    }

    private sealed class FakeLootService : ILootService
    {
        private readonly IReadOnlyList<ItemEntity> _generatedItems;

        public FakeLootService(IReadOnlyList<ItemEntity> generatedItems)
        {
            _generatedItems = generatedItems;
        }

        public int GenerateCalls { get; private set; }

        public ValueTask<IReadOnlyList<ItemEntity>> GenerateAsync(
            string lootTableId,
            CancellationToken cancellationToken = default
        )
        {
            Assert.Equal("common", lootTableId);
            GenerateCalls++;

            return ValueTask.FromResult(_generatedItems);
        }

        public bool Has(string lootTableId)
        {
            return string.Equals(lootTableId, "common", StringComparison.OrdinalIgnoreCase);
        }
    }

    private sealed class FakeItemService : IItemService
    {
        private readonly HashSet<int> _containerItemIds;

        public FakeItemService(params int[] containerItemIds)
        {
            _containerItemIds = [.. containerItemIds];
        }

        public List<(ItemEntity Container, ItemEntity Child, Point2D Position)> Added { get; } = [];

        public ValueTask<bool> AddItemAsync(
            ItemEntity container,
            ItemEntity child,
            Point2D position,
            CancellationToken cancellationToken = default
        )
        {
            child.ParentContainerId = container.Id;
            child.ContainerPosition = position;
            container.ContainedItemIds.Add(child.Id);
            Added.Add((container, child, position));

            return ValueTask.FromResult(true);
        }

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public ValueTask<ItemEntity> CreateAsync(ItemEntity item, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public ValueTask<ItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public bool IsContainer(ItemEntity item)
        {
            return IsContainer(item.ItemId);
        }

        public bool IsContainer(int itemId)
        {
            return _containerItemIds.Contains(itemId);
        }

        public bool IsDoor(ItemEntity item)
        {
            throw new NotSupportedException();
        }

        public bool IsDoor(int itemId)
        {
            throw new NotSupportedException();
        }

        public ValueTask<bool> RemoveItemAsync(
            ItemEntity container,
            Serial itemId,
            CancellationToken cancellationToken = default
        )
        {
            throw new NotSupportedException();
        }

        public ValueTask<int> TotalWeightAsync(ItemEntity item, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeContainerDataService : IContainerDataService
    {
        private readonly IReadOnlyList<ContainerLayoutEntry> _layouts;

        public FakeContainerDataService(params ContainerLayoutEntry[] layouts)
        {
            _layouts = layouts;
        }

        public bool IsLazy => false;

        public bool IsLoaded => true;

        public void EnsureLoaded()
        {
        }

        public IReadOnlyList<ContainerEntry> GetAllContainers()
        {
            return [];
        }

        public IReadOnlyList<ContainerLayoutEntry> GetAllLayouts()
        {
            return _layouts;
        }

        public void Reload()
        {
        }

        public void SetContainers(IReadOnlyList<ContainerEntry> entries)
        {
        }

        public void SetLayouts(IReadOnlyList<ContainerLayoutEntry> entries)
        {
        }
    }
}
