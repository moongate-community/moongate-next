using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Server.Services.World;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Tests.Server.World;

public sealed class WorldSpatialIndexTests
{
    [Fact]
    public void AddMobile_Get_Remove_RoundTrips()
    {
        var index = new WorldSpatialIndex();
        var mobile = new MobileEntity { Id = new Serial(7), MapId = 0, Location = new Point3D(50, 50, 0) };

        index.AddMobile(mobile);

        Assert.True(index.TryGet(new Serial(7), out var found));
        Assert.Same(mobile, found);
        Assert.Single(index.All);

        Assert.True(index.RemoveMobile(new Serial(7)));
        Assert.False(index.TryGet(new Serial(7), out _));
        Assert.Empty(index.All);
        Assert.False(index.RemoveMobile(new Serial(7)));
    }

    [Fact]
    public void MoveMobile_SetsLocation_AndRebucketsAcrossSectorBoundary()
    {
        var index = new WorldSpatialIndex();
        var mobile = new MobileEntity { Id = new Serial(7), MapId = 0, Location = new Point3D(15, 15, 0) };
        index.AddMobile(mobile);

        index.MoveMobile(mobile, new Point3D(16, 15, 0)); // crosses x sector boundary (15>>4=0, 16>>4=1)

        Assert.Equal(new Point3D(16, 15, 0), mobile.Location);
        Assert.True(index.TryGet(new Serial(7), out var found));
        Assert.Same(mobile, found);
        Assert.Single(index.All);
    }

    [Fact]
    public void GetMobilesInRange_GathersAdjacentSectors_AndFiltersByDistance()
    {
        var index = new WorldSpatialIndex();
        var near = new MobileEntity { Id = new Serial(1), MapId = 0, Location = new Point3D(16, 16, 0) }; // sector (1,1)
        var alsoNear = new MobileEntity
        { Id = new Serial(2), MapId = 0, Location = new Point3D(14, 16, 0) }; // sector (0,1), distance 2
        var far = new MobileEntity { Id = new Serial(3), MapId = 0, Location = new Point3D(40, 40, 0) };
        var otherMap = new MobileEntity { Id = new Serial(4), MapId = 1, Location = new Point3D(16, 16, 0) };
        index.AddMobile(near);
        index.AddMobile(alsoNear);
        index.AddMobile(far);
        index.AddMobile(otherMap);

        var found = index.GetMobilesInRange(0, new Point3D(16, 16, 0), 3);

        Assert.Contains(near, found);
        Assert.Contains(alsoNear, found);
        Assert.DoesNotContain(far, found);
        Assert.DoesNotContain(otherMap, found);
    }

    [Fact]
    public void GetPlayersInRange_ExcludesNpcs()
    {
        var index = new WorldSpatialIndex();
        var player = new MobileEntity { Id = new Serial(1), MapId = 0, Location = new Point3D(50, 50, 0), IsPlayer = true };
        var npc = new MobileEntity { Id = new Serial(2), MapId = 0, Location = new Point3D(50, 50, 0), IsPlayer = false };
        index.AddMobile(player);
        index.AddMobile(npc);

        var players = index.GetPlayersInRange(0, new Point3D(50, 50, 0), 5);

        Assert.Contains(player, players);
        Assert.DoesNotContain(npc, players);
    }

    [Fact]
    public void GetItemsInRange_ReturnsGroundItemsInRange()
    {
        var index = new WorldSpatialIndex();
        var item = new ItemEntity { Id = new Serial(0x40000001), MapId = 0, Location = new Point3D(50, 50, 0) };
        index.AddOrUpdateItem(item);

        Assert.Contains(item, index.GetItemsInRange(0, new Point3D(51, 50, 0), 2));
        Assert.DoesNotContain(item, index.GetItemsInRange(0, new Point3D(60, 60, 0), 2));
    }

    [Fact]
    public void AddMobile_SameSerialTwice_Replaces_NoDuplicate()
    {
        var index = new WorldSpatialIndex();
        var first = new MobileEntity { Id = new Serial(7), MapId = 0, Location = new Point3D(10, 10, 0) };
        var second = new MobileEntity
        { Id = new Serial(7), MapId = 0, Location = new Point3D(80, 80, 0) }; // different sector

        index.AddMobile(first);
        index.AddMobile(second);

        Assert.Single(index.All);
        Assert.True(index.TryGet(new Serial(7), out var found));
        Assert.Same(second, found);
        Assert.Contains(second, index.GetMobilesInRange(0, new Point3D(80, 80, 0), 1));
        Assert.DoesNotContain(first, index.GetMobilesInRange(0, new Point3D(10, 10, 0), 1)); // old bucket cleared
    }

    [Fact]
    public void MoveMobile_WithinSameSector_UpdatesLocation_StillQueryable()
    {
        var index = new WorldSpatialIndex();
        var mobile = new MobileEntity { Id = new Serial(7), MapId = 0, Location = new Point3D(10, 10, 0) }; // sector (0,0)
        index.AddMobile(mobile);

        index.MoveMobile(mobile, new Point3D(12, 12, 0)); // same sector (0,0)

        Assert.Equal(new Point3D(12, 12, 0), mobile.Location);
        Assert.Single(index.All);
        Assert.Contains(mobile, index.GetMobilesInRange(0, new Point3D(12, 12, 0), 1));
    }

    [Fact]
    public void RemoveItem_RemovesFromQueries_AndIsIdempotent()
    {
        var index = new WorldSpatialIndex();
        var item = new ItemEntity { Id = new Serial(0x40000001), MapId = 0, Location = new Point3D(50, 50, 0) };
        index.AddOrUpdateItem(item);

        Assert.True(index.RemoveItem(new Serial(0x40000001)));
        Assert.Empty(index.GetItemsInRange(0, new Point3D(50, 50, 0), 2));
        Assert.False(index.RemoveItem(new Serial(0x40000001)));
    }
}
