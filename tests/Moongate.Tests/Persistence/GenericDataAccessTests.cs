using Moongate.Core.Ids;
using Moongate.Persistence.Data;
using Moongate.Persistence.Internal;
using Moongate.Persistence.Services.Persistence;
using Moongate.Persistence.Types;
using Moongate.Tests.Persistence.Support;

namespace Moongate.Tests.Persistence;

public class GenericDataAccessTests
{
    [Fact]
    public async Task GetAll_ReturnsDetachedClones()
    {
        var access = NewAccess(out _, out _);
        await access.UpsertAsync(new() { Id = new(1), Name = "original" });

        var all = await access.GetAllAsync();
        all.First().Name = "mutated";

        Assert.Equal("original", (await access.GetByIdAsync(new(1)))!.Name);
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNull()
    {
        var access = NewAccess(out _, out _);

        Assert.Null(await access.GetByIdAsync(new(999)));
    }

    [Fact]
    public async Task Query_ReturnsDetachedQueryableClones()
    {
        var access = NewAccess(out _, out _);
        await access.UpsertAsync(new() { Id = new(1), Name = "active", Level = 10 });
        await access.UpsertAsync(new() { Id = new(2), Name = "inactive", Level = 1 });

        var queried = access.Query().Where(player => player.Level >= 10).ToArray();
        queried[0].Name = "mutated";

        Assert.Single(queried);
        Assert.Equal("active", (await access.GetByIdAsync(new(1)))!.Name);
    }

    [Fact]
    public async Task Remove_Existing_ReturnsTrueAndAppendsRemoveEntry()
    {
        var access = NewAccess(out var journal, out _);
        await access.UpsertAsync(new() { Id = new(1), Name = "a" });

        var removed = await access.RemoveAsync(new(1));

        Assert.True(removed);
        Assert.Equal(0, await access.CountAsync());
        Assert.Equal(JournalEntityOperationType.Remove, journal.Entries[^1].Operation);
    }

    [Fact]
    public async Task Remove_Missing_ReturnsFalseAndAppendsNothing()
    {
        var access = NewAccess(out var journal, out _);

        var removed = await access.RemoveAsync(new(123));

        Assert.False(removed);
        Assert.Empty(journal.Entries);
    }

    [Fact]
    public async Task Upsert_AppendsUpsertJournalEntryWithSequenceId()
    {
        var access = NewAccess(out var journal, out _);

        await access.UpsertAsync(new() { Id = new(1), Name = "a" });

        Assert.Single(journal.Entries);
        Assert.Equal(JournalEntityOperationType.Upsert, journal.Entries[0].Operation);
        Assert.Equal(1, journal.Entries[0].SequenceId);
        Assert.Equal((ushort)1, journal.Entries[0].TypeId);
    }

    [Fact]
    public async Task Upsert_SameKeyTwice_OverwritesAndCountsOne()
    {
        var access = NewAccess(out _, out _);

        await access.UpsertAsync(new() { Id = new(1), Name = "v1" });
        await access.UpsertAsync(new() { Id = new(1), Name = "v2" });

        Assert.Equal(1, await access.CountAsync());
        Assert.Equal("v2", (await access.GetByIdAsync(new(1)))!.Name);
    }

    [Fact]
    public async Task Upsert_ThenGetById_ReturnsEntity()
    {
        var access = NewAccess(out _, out _);

        await access.UpsertAsync(new() { Id = new(1), Name = "Bob", Level = 3 });
        var result = await access.GetByIdAsync(new(1));

        Assert.NotNull(result);
        Assert.Equal("Bob", result!.Name);
    }

    private static GenericDataAccess<TestPlayer, Serial> NewAccess(
        out InMemoryJournalService journal,
        out PersistenceStateStore store
    )
    {
        journal = new();
        store = new();
        var descriptor = new PersistenceEntityDescriptor<TestPlayer, Serial>(1, "TestPlayer", 1, p => p.Id);

        return new(store, journal, descriptor);
    }
}
