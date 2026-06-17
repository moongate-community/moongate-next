using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Server.Services.Items;
using Moongate.Server.Services.World;
using Moongate.Tests.Server.Items.Support;
using Moongate.UO.Data.Entities.Items;

namespace Moongate.Tests.Server.Items;

public sealed class ItemServiceSpatialIndexTests
{
    [Fact]
    public async Task CreateAsync_GroundItem_IsAddedToSpatialIndex()
    {
        var index = new WorldSpatialIndex();
        var service = new ItemService(new FakeItemAccess(), new FakeTileDataStore(), index);
        var item = new ItemEntity { Id = new Serial(0x40000001), MapId = 0, Location = new Point3D(50, 50, 0) }; // ground

        await service.CreateAsync(item);

        Assert.Contains(item, index.GetItemsInRange(0, new Point3D(50, 50, 0), range: 1));
    }

    [Fact]
    public async Task DeleteAsync_RemovesItemFromSpatialIndex()
    {
        var index = new WorldSpatialIndex();
        var service = new ItemService(new FakeItemAccess(), new FakeTileDataStore(), index);
        var item = new ItemEntity { Id = new Serial(0x40000002), MapId = 0, Location = new Point3D(50, 50, 0) };
        await service.CreateAsync(item);

        await service.DeleteAsync(item.Id);

        Assert.Empty(index.GetItemsInRange(0, new Point3D(50, 50, 0), range: 1));
    }
}
