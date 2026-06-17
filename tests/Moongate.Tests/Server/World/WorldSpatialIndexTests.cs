using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Server.Services.World;
using Moongate.UO.Data.Entities.Mobiles;
using Xunit;

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
}
