using Moongate.Core.Ids;
using Moongate.Persistence.Internal;
using Moongate.Tests.Persistence.Support;

namespace Moongate.Tests.Persistence;

public class PersistenceStateStoreTests
{
    [Fact]
    public void ClearBuckets_RemovesAll()
    {
        var store = new PersistenceStateStore();
        store.GetBucket<TestPlayer, Serial>(1)[new Serial(1)] = new TestPlayer { Id = new Serial(1) };

        store.ClearBuckets();

        Assert.Empty(store.GetBucket<TestPlayer, Serial>(1));
    }

    [Fact]
    public void GetBucket_DifferentTypeIds_AreIsolated()
    {
        var store = new PersistenceStateStore();

        store.GetBucket<TestPlayer, Serial>(1)[new Serial(7)] = new TestPlayer { Id = new Serial(7) };

        Assert.Empty(store.GetBucket<TestItem, Serial>(2));
        Assert.Single(store.GetBucket<TestPlayer, Serial>(1));
    }

    [Fact]
    public void GetBucket_SameTypeId_ReturnsSameInstance()
    {
        var store = new PersistenceStateStore();

        var a = store.GetBucket<TestPlayer, Serial>(1);
        var b = store.GetBucket<TestPlayer, Serial>(1);

        Assert.Same(a, b);
    }
}
