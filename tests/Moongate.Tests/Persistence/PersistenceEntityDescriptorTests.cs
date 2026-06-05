using Moongate.Core.Ids;
using Moongate.Persistence.Data;
using Moongate.Tests.Persistence.Support;

namespace Moongate.Tests.Persistence;

public class PersistenceEntityDescriptorTests
{
    [Fact]
    public void Clone_ProducesIndependentCopy()
    {
        var descriptor = NewDescriptor();
        var original = new TestPlayer { Id = new(1), Name = "X", Level = 1 };

        var clone = descriptor.Clone(original);
        clone.Name = "Y";

        Assert.Equal("X", original.Name);
    }

    [Fact]
    public void Descriptor_ExposesTypeMetadata()
    {
        var descriptor = NewDescriptor();

        Assert.Equal((ushort)1, descriptor.TypeId);
        Assert.Equal("TestPlayer", descriptor.TypeName);
        Assert.Equal(typeof(TestPlayer), descriptor.EntityType);
        Assert.Equal(typeof(Serial), descriptor.KeyType);
    }

    [Fact]
    public void GetKey_ReturnsSelectedKey()
    {
        var descriptor = NewDescriptor();

        Assert.Equal(new(7), descriptor.GetKey(new() { Id = new(7), Name = "a" }));
    }

    [Fact]
    public void SerializeBucket_DeserializeBucket_RoundTripsAll()
    {
        var descriptor = NewDescriptor();
        IReadOnlyCollection<TestPlayer> players =
            [new() { Id = new(1), Name = "a" }, new() { Id = new(2), Name = "b" }];

        var back = descriptor.DeserializeBucket(descriptor.SerializeBucket(players));

        Assert.Equal(2, back.Count);
    }

    [Fact]
    public void SerializeEntity_DeserializeEntity_RoundTrips()
    {
        var descriptor = NewDescriptor();
        var player = new TestPlayer { Id = new(3), Name = "Bob", Level = 12 };

        var back = descriptor.DeserializeEntity(descriptor.SerializeEntity(player));

        Assert.Equal(new(3), back.Id);
        Assert.Equal("Bob", back.Name);
        Assert.Equal(12, back.Level);
    }

    [Fact]
    public void SerializeKey_DeserializeKey_RoundTrips()
    {
        var descriptor = NewDescriptor();

        Assert.Equal(new(99), descriptor.DeserializeKey(descriptor.SerializeKey(new(99))));
    }

    private static PersistenceEntityDescriptor<TestPlayer, Serial> NewDescriptor()
        => new(1, "TestPlayer", 1, p => p.Id);
}
