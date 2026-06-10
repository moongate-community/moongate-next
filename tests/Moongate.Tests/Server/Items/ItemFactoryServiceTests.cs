using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Server.Services.Items;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Properties;

namespace Moongate.Tests.Server.Items;

public sealed class ItemFactoryServiceTests
{
    private sealed class FakeItemService : IItemService
    {
        private uint _next = Serial.ItemOffset + 1;

        public List<ItemEntity> Created { get; } = [];

        public ValueTask<ItemEntity> CreateAsync(ItemEntity item, CancellationToken cancellationToken = default)
        {
            if (!item.Id.IsValid)
            {
                item.Id = new Serial(_next++);
            }

            Created.Add(item);

            return ValueTask.FromResult(item);
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

        public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<ItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public bool IsContainer(ItemEntity item)
            => throw new NotSupportedException();

        public bool IsContainer(int itemId)
            => throw new NotSupportedException();

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

    private static ItemTemplateService NewRegistry(params ItemTemplateDefinition[] templates)
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange(templates);

        return registry;
    }

    [Fact]
    public async Task CreateFromTemplateAsync_WithAmount_SetsAmountOnEntity()
    {
        var template = new ItemTemplateDefinition { Id = "gold_coin", ItemId = 3821, IsStackable = true };
        var items = new FakeItemService();
        var factory = new ItemFactoryService(NewRegistry(template), items);

        var entity = await factory.CreateFromTemplateAsync("gold_coin", 250);

        Assert.Equal(250, entity.Amount);
        Assert.True(entity.Id.IsValid);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_MapsTemplateFieldsOntoEntity()
    {
        var template = new ItemTemplateDefinition
        {
            Id = "plain_shirt",
            Name = "Shirt",
            ItemId = 5399,
            Hue = 33,
            Weight = 1,
            Amount = 1,
            IsStackable = false,
            GumpId = 0x3C,
            ScriptId = "shirt_script",
            Rarity = ItemRarity.Common,
            Visibility = UserLevelType.GameMaster
        };
        var items = new FakeItemService();
        var factory = new ItemFactoryService(NewRegistry(template), items);

        var entity = await factory.CreateFromTemplateAsync("plain_shirt");

        Assert.True(entity.Id.IsValid);
        Assert.Equal("Shirt", entity.Name);
        Assert.Equal(5399, entity.ItemId);
        Assert.Equal((Hue)33, entity.Hue);
        Assert.Equal(1, entity.Weight);
        Assert.Equal(1, entity.Amount);
        Assert.False(entity.IsStackable);
        Assert.Equal(0x3C, entity.GumpId);
        Assert.Equal("shirt_script", entity.ScriptId);
        Assert.Equal(ItemRarity.Common, entity.Rarity);
        Assert.Equal(UserLevelType.GameMaster, entity.Visibility);
        Assert.Single(items.Created);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_WritesIsMovableAndParamsToCustomProperties()
    {
        var template = new ItemTemplateDefinition
        {
            Id = "shirt",
            IsMovable = true
        };
        template.Params["dyeable"] = new ItemTemplateParamDefinition
        {
            Type = ItemTemplateParamType.String,
            Value = "true"
        };
        template.Params["charges"] = new ItemTemplateParamDefinition
        {
            Type = ItemTemplateParamType.Integer,
            Value = "0x10"
        };
        var factory = new ItemFactoryService(NewRegistry(template), new FakeItemService());

        var entity = await factory.CreateFromTemplateAsync("shirt");

        Assert.Equal(CustomPropertyType.Boolean, entity.CustomProperties["is_movable"].Type);
        Assert.True(entity.CustomProperties["is_movable"].BooleanValue);
        Assert.Equal(CustomPropertyType.String, entity.CustomProperties["dyeable"].Type);
        Assert.Equal("true", entity.CustomProperties["dyeable"].StringValue);
        Assert.Equal(CustomPropertyType.Integer, entity.CustomProperties["charges"].Type);
        Assert.Equal(16L, entity.CustomProperties["charges"].IntegerValue);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_UnknownTemplate_Throws()
    {
        var factory = new ItemFactoryService(NewRegistry(), new FakeItemService());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => factory.CreateFromTemplateAsync("missing").AsTask()
        );
        Assert.Contains("missing", exception.Message);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_AbstractTemplate_Throws()
    {
        var template = new ItemTemplateDefinition
        {
            Id = "base_clothing",
            IsAbstract = true
        };
        var factory = new ItemFactoryService(NewRegistry(template), new FakeItemService());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => factory.CreateFromTemplateAsync("base_clothing").AsTask()
        );
        Assert.Contains("abstract", exception.Message);
    }
}
