using Moongate.Core.Ids;
using Moongate.Persistence.Data;
using Moongate.Persistence.Internal;
using Moongate.Persistence.Services.Persistence;
using Moongate.Tests.Persistence.Support;

namespace Moongate.Tests.Persistence;

public class AutoDataAccessTests
{
    [Fact]
    public async Task NextId_AutoInt32_AfterUpsert_ContinuesFromMaxId()
    {
        var access = NewInt32Access(out _);
        await access.UpsertAsync(new() { Id = new(7), Name = "x" });

        var next = await access.NextIdAsync();

        Assert.Equal(new(8), next);
    }

    // --- AutoInt32 key ---

    [Fact]
    public async Task NextId_AutoInt32_FirstCall_Returns1()
    {
        var access = NewInt32Access(out _);

        var id = await access.NextIdAsync();

        Assert.Equal(new(1), id);
    }

    [Fact]
    public async Task NextId_AutoInt64_AfterUpsert_ContinuesFromMaxId()
    {
        var access = NewInt64Access(out _);
        await access.UpsertAsync(new() { Id = new(100), Name = "y" });

        var next = await access.NextIdAsync();

        Assert.Equal(new(101), next);
    }

    // --- AutoInt64 key ---

    [Fact]
    public async Task NextId_AutoInt64_FirstCall_Returns1()
    {
        var access = NewInt64Access(out _);

        var id = await access.NextIdAsync();

        Assert.Equal(new(1), id);
    }

    [Fact]
    public async Task NextId_Serial_AfterRemove_DoesNotReuseId()
    {
        var access = NewSerialAccess(out _);
        await access.UpsertAsync(new() { Id = new(5), Name = "a" });
        await access.RemoveAsync(new(5));

        var next = await access.NextIdAsync();

        Assert.Equal(new(6), next);
    }

    [Fact]
    public async Task NextId_Serial_AfterUpsert_ContinuesFromMaxId()
    {
        var access = NewSerialAccess(out _);
        await access.UpsertAsync(new() { Id = new(10), Name = "a" });

        var next = await access.NextIdAsync();

        Assert.Equal(new(11), next);
    }

    [Fact]
    public async Task NextId_Serial_CalledTwice_ReturnsConsecutiveSerials()
    {
        var access = NewSerialAccess(out _);

        var first = await access.NextIdAsync();
        var second = await access.NextIdAsync();

        Assert.Equal(new(1), first);
        Assert.Equal(new(2), second);
    }

    // --- Serial key ---

    [Fact]
    public async Task NextId_Serial_FirstCall_ReturnsSerial1()
    {
        var access = NewSerialAccess(out _);

        var id = await access.NextIdAsync();

        Assert.Equal(new(1), id);
    }

    [Fact]
    public async Task NextId_Serial_UpsertWithAllocatedId_StoredCorrectly()
    {
        var access = NewSerialAccess(out _);

        var id = await access.NextIdAsync();
        await access.UpsertAsync(new() { Id = id, Name = "auto" });

        var entity = await access.GetByIdAsync(id);
        Assert.NotNull(entity);
        Assert.Equal("auto", entity!.Name);
    }

    private static AutoDataAccess<TestPlayerInt32, AutoInt32> NewInt32Access(out PersistenceStateStore store)
    {
        store = new();
        var journal = new InMemoryJournalService();
        var descriptor = new PersistenceEntityDescriptor<TestPlayerInt32, AutoInt32>(2, "TestPlayerInt32", 1, p => p.Id);

        return new(store, journal, descriptor);
    }

    private static AutoDataAccess<TestPlayerInt64, AutoInt64> NewInt64Access(out PersistenceStateStore store)
    {
        store = new();
        var journal = new InMemoryJournalService();
        var descriptor = new PersistenceEntityDescriptor<TestPlayerInt64, AutoInt64>(3, "TestPlayerInt64", 1, p => p.Id);

        return new(store, journal, descriptor);
    }

    private static AutoDataAccess<TestPlayer, Serial> NewSerialAccess(out PersistenceStateStore store)
    {
        store = new();
        var journal = new InMemoryJournalService();
        var descriptor = new PersistenceEntityDescriptor<TestPlayer, Serial>(1, "TestPlayer", 1, p => p.Id);

        return new(store, journal, descriptor);
    }
}
