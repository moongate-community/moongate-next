using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Server.Services.Loot;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Loot;

namespace Moongate.Tests.Server.Templates;

public sealed class ItemTemplateContentsValidatorTests
{
    [Fact]
    public void Validate_ValidContainerContents_Passes()
    {
        var templates = new[] { Template("wooden_chest", 3651, "common") };
        var registry = Registry("common", templates);
        var items = new FakeItemService(3651);

        ItemTemplateContentsValidator.Validate(templates, registry, items);
    }

    [Fact]
    public void Validate_UnknownLootTemplate_Throws()
    {
        var templates = new[] { Template("wooden_chest", 3651, "missing") };
        var registry = Registry("common", templates);
        var items = new FakeItemService(3651);

        var exception = Assert.Throws<InvalidOperationException>(
            () => ItemTemplateContentsValidator.Validate(templates, registry, items)
        );

        Assert.Contains("wooden_chest", exception.Message);
        Assert.Contains("missing", exception.Message);
    }

    [Fact]
    public void Validate_NonContainerTemplate_Throws()
    {
        var templates = new[] { Template("wooden_chest", 3651, "common") };
        var registry = Registry("common", templates);
        var items = new FakeItemService();

        var exception = Assert.Throws<InvalidOperationException>(
            () => ItemTemplateContentsValidator.Validate(templates, registry, items)
        );

        Assert.Contains("wooden_chest", exception.Message);
        Assert.Contains("container", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_NonPositiveRefillEvery_Throws(int hours)
    {
        var templates = new[] { Template("wooden_chest", 3651, "common", TimeSpan.FromHours(hours)) };
        var registry = Registry("common", templates);
        var items = new FakeItemService(3651);

        var exception = Assert.Throws<InvalidOperationException>(
            () => ItemTemplateContentsValidator.Validate(templates, registry, items)
        );

        Assert.Contains("wooden_chest", exception.Message);
        Assert.Contains("refill_every", exception.Message);
    }

    private static LootTableRegistry Registry(string id, IEnumerable<ItemTemplateDefinition> templates)
        => new(
            [
                new()
                {
                    Id = id,
                    Content = [new() { Item = "gold_coin" }]
                }
            ],
            templates
        );

    private static ItemTemplateDefinition Template(
        string id,
        int itemId,
        string lootTemplate,
        TimeSpan? refillEvery = null
    )
        => new()
        {
            Id = id,
            ItemId = itemId,
            Contents = new()
            {
                LootTemplate = lootTemplate,
                RefillEvery = refillEvery ?? TimeSpan.FromHours(6)
            }
        };

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
}
