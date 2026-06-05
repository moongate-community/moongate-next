using Moongate.Abstractions.Data.Network;

namespace Moongate.Tests.Hosting.Network;

public class NetworkConfigTests
{
    [Fact]
    public void Defaults_MatchUoConventions()
    {
        var config = new NetworkConfig();

        Assert.Equal(2593, config.Port);
        Assert.True(config.PingServerEnabled);
        Assert.Equal(12000, config.PingServerPort);
        Assert.Equal(64 * 1024, config.MaxPendingBufferBytes);
        Assert.Equal(16 * 1024, config.MaxDeclaredPacketLength);
        Assert.Equal(256, config.MaxPacketsPerDrain);
        Assert.Equal(256, config.MaxOutgoingPacketsPerDrain);
    }
}
