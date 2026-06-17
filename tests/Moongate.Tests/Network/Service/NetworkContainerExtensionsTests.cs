using DryIoc;
using Moongate.Network.UO.Registry;
using Moongate.Server.Extensions.Network;
using Moongate.Server.Interfaces.Network;

namespace Moongate.Tests.Network.Service;

public class NetworkContainerExtensionsTests : IDisposable
{
    private readonly IContainer _container = new Container();

    public void Dispose()
    {
        _container.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void AddMoongateNetwork_RegistersOutgoingPacketQueue()
    {
        _container.RegisterInstance(new PacketRegistry());

        _container.AddMoongateNetwork();

        Assert.NotNull(_container.Resolve<IOutgoingPacketQueue>());
    }
}
