using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Server.Services.Items;
using Moongate.Server.Services.World;
using Moongate.Tests.Server.Items.Support;
using Moongate.UO.Data.Data;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Types.Properties;

namespace Moongate.Tests.Server.Items;

public sealed class ItemServiceTests
{
    private const int BackpackItemId = 0x0E75;
    private const int DoorItemId = 0x0675;
    private const int ArrowItemId = 0x1BFB;
    private const int ChildCount = 50;

    [Fact]
    public async Task AddItemAsync_NonContainer_ReturnsFalse()
    {
        var access = new FakeItemAccess();
        var service = new ItemService(access, new FakeTileDataStore(), new WorldSpatialIndex());

        var plain = await service.CreateAsync(new ItemEntity { ItemId = ArrowItemId });
        var child = await service.CreateAsync(new ItemEntity { ItemId = ArrowItemId });

        Assert.False(await service.AddItemAsync(plain, child, new Point2D(0, 0)));
        Assert.Empty(plain.ContainedItemIds);
    }

    [Fact]
    public async Task AddItemAsync_Stores50ChildrenWithCustomProperties_UnderContainer()
    {
        var access = new FakeItemAccess();
        var tiles = new FakeTileDataStore();
        tiles.Container(BackpackItemId);
        var service = new ItemService(access, tiles, new WorldSpatialIndex());

        var backpack = await service.CreateAsync(new ItemEntity { ItemId = BackpackItemId, Weight = 3 });

        for (var i = 0; i < ChildCount; i++)
        {
            var child = new ItemEntity { ItemId = ArrowItemId, Weight = 1, Amount = 1 };
            child.CustomProperties["index"] = new CustomProperty
            {
                Type = CustomPropertyType.Integer,
                IntegerValue = i
            };

            await service.CreateAsync(child);
            var added = await service.AddItemAsync(backpack, child, new Point2D(i, 0));

            Assert.True(added);
            Assert.Equal(backpack.Id, child.ParentContainerId);
        }

        Assert.Equal(ChildCount, backpack.ContainedItemIds.Count);
        Assert.Equal(ChildCount + 1, await access.CountAsync());

        // Every child persisted with a distinct item-range serial and its custom property.
        var distinctIds = backpack.ContainedItemIds.Distinct().Count();
        Assert.Equal(ChildCount, distinctIds);
        Assert.All(backpack.ContainedItemIds, id => Assert.True(id.Value >= Serial.ItemOffset));

        var firstChild = await access.GetByIdAsync(backpack.ContainedItemIds[0]);
        Assert.NotNull(firstChild);
        Assert.Equal(0, firstChild!.CustomProperties["index"].IntegerValue);

        // TotalWeight aggregates the container plus all contained children.
        var total = await service.TotalWeightAsync(backpack);
        Assert.Equal(3 + ChildCount, total);
    }

    [Fact]
    public async Task CreateAsync_AllocatesSerialInItemRange()
    {
        var service = new ItemService(new FakeItemAccess(), new FakeTileDataStore(), new WorldSpatialIndex());

        var item = await service.CreateAsync(new ItemEntity { ItemId = ArrowItemId, Weight = 1 });

        Assert.True(item.Id.Value >= Serial.ItemOffset);
    }

    [Fact]
    public void IsContainer_And_IsDoor_DerivedFromTileData()
    {
        var tiles = new FakeTileDataStore();
        tiles.Container(BackpackItemId);
        tiles.MakeDoor(DoorItemId);
        var service = new ItemService(new FakeItemAccess(), tiles, new WorldSpatialIndex());

        Assert.True(service.IsContainer(new ItemEntity { ItemId = BackpackItemId }));
        Assert.False(service.IsContainer(new ItemEntity { ItemId = ArrowItemId }));
        Assert.True(service.IsDoor(new ItemEntity { ItemId = DoorItemId }));
        Assert.False(service.IsDoor(new ItemEntity { ItemId = ArrowItemId }));

        // id-only overloads
        Assert.True(service.IsContainer(BackpackItemId));
        Assert.False(service.IsContainer(ArrowItemId));
        Assert.True(service.IsDoor(DoorItemId));
        Assert.False(service.IsDoor(ArrowItemId));
    }

    [Fact]
    public async Task RemoveItemAsync_DetachesChildAndClearsParent()
    {
        var access = new FakeItemAccess();
        var tiles = new FakeTileDataStore();
        tiles.Container(BackpackItemId);
        var service = new ItemService(access, tiles, new WorldSpatialIndex());

        var backpack = await service.CreateAsync(new ItemEntity { ItemId = BackpackItemId });
        var child = await service.CreateAsync(new ItemEntity { ItemId = ArrowItemId });
        await service.AddItemAsync(backpack, child, new Point2D(0, 0));

        var removed = await service.RemoveItemAsync(backpack, child.Id);

        Assert.True(removed);
        Assert.Empty(backpack.ContainedItemIds);
        Assert.Equal(default, (await access.GetByIdAsync(child.Id))!.ParentContainerId);
    }
}
