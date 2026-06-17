using Moongate.Core.Ids;
using Moongate.Server.Services.World;
using Moongate.UO.Data.Entities.Mobiles;
using Xunit;

namespace Moongate.Tests.Server.World;

public sealed class WorldMobileRegistryTests
{
    [Fact]
    public void Add_Get_Remove_RoundTrips()
    {
        var registry = new WorldMobileRegistry();
        var mobile = new MobileEntity { Id = new Serial(7) };

        registry.Add(mobile);

        Assert.True(registry.TryGet(new Serial(7), out var found));
        Assert.Same(mobile, found);
        Assert.Single(registry.All);

        Assert.True(registry.Remove(new Serial(7)));
        Assert.False(registry.TryGet(new Serial(7), out _));
        Assert.Empty(registry.All);
        Assert.False(registry.Remove(new Serial(7)));
    }
}
