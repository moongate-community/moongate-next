using Moongate.Core.Ids;
using Moongate.Persistence.Data;
using Moongate.Persistence.Services.Persistence;
using Moongate.Tests.Persistence.Support;

namespace Moongate.Tests.Persistence;

public class PersistenceEntityRegistryTests
{
    [Fact]
    public void GetDescriptor_UnknownTypeId_Throws()
    {
        var registry = new PersistenceEntityRegistry();

        Assert.Throws<InvalidOperationException>(() => registry.GetDescriptor(99));
    }

    [Fact]
    public void GetDescriptorGeneric_UnregisteredPair_Throws()
    {
        var registry = new PersistenceEntityRegistry();

        Assert.Throws<InvalidOperationException>(() => registry.GetDescriptor<TestPlayer, Serial>());
    }

    [Fact]
    public void Register_AfterFreeze_Throws()
    {
        var registry = new PersistenceEntityRegistry();
        registry.Freeze();

        Assert.True(registry.IsFrozen);
        Assert.Throws<InvalidOperationException>(() => registry.Register(PlayerDescriptor()));
    }

    [Fact]
    public void Register_DuplicateTypeId_Throws()
    {
        var registry = new PersistenceEntityRegistry();
        registry.Register(PlayerDescriptor());

        Assert.Throws<InvalidOperationException>(
            () => registry.Register(new PersistenceEntityDescriptor<TestItem, Serial>(1, "Dup", 1, i => i.Id))
        );
    }

    [Fact]
    public void Register_ThenGetDescriptorByTypeId_ReturnsIt()
    {
        var registry = new PersistenceEntityRegistry();
        registry.Register(PlayerDescriptor());

        Assert.Equal("TestPlayer", registry.GetDescriptor(1).TypeName);
    }

    [Fact]
    public void Register_TwoTypes_BothResolvableByGenericLookup()
    {
        var registry = new PersistenceEntityRegistry();
        registry.Register(PlayerDescriptor());
        registry.Register(ItemDescriptor());

        Assert.True(registry.IsRegistered<TestPlayer, Serial>());
        Assert.True(registry.IsRegistered<TestItem, Serial>());
        Assert.Equal(2, registry.GetRegisteredDescriptors().Count);
    }

    private static PersistenceEntityDescriptor<TestItem, Serial> ItemDescriptor()
        => new(2, "TestItem", 1, i => i.Id);

    private static PersistenceEntityDescriptor<TestPlayer, Serial> PlayerDescriptor()
        => new(1, "TestPlayer", 1, p => p.Id);
}
