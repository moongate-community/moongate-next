using DryIoc;
using Moongate.Server.Extensions.Items;
using Moongate.Server.Interfaces.Services.Items;

namespace Moongate.Tests.Server.Items;

public sealed class ItemContainerExtensionsTests
{
    [Fact]
    public void AddMoongateItems_RegistersContainerContentService()
    {
        using var container = new Container();

        container.AddMoongateItems();

        Assert.True(container.IsRegistered<IContainerContentService>());
    }
}
