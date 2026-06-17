using Moongate.Persistence.Data;
using Moongate.Persistence.Services.Persistence;
using Moongate.Persistence.Types;

namespace Moongate.Tests.Persistence;

public class BinaryJournalServiceTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"nh-journal-{Guid.NewGuid():N}.bin");

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AppendBatch_WritesAll()
    {
        await using var journal = new BinaryJournalService(_path, false);
        await journal.AppendBatchAsync([Entry(1), Entry(2), Entry(3)]);

        Assert.Equal(3, (await journal.ReadAllAsync()).Count);
    }

    [Fact]
    public async Task AppendThenReadAll_RoundTripsInOrder()
    {
        await using var journal = new BinaryJournalService(_path, false);
        await journal.AppendAsync(Entry(1));
        await journal.AppendAsync(Entry(2));

        var all = (await journal.ReadAllAsync()).ToArray();

        Assert.Equal(2, all.Length);
        Assert.Equal(1, all[0].SequenceId);
        Assert.Equal(2, all[1].SequenceId);
    }

    [Fact]
    public async Task ReadAll_AfterReopen_SeesPersistedEntries()
    {
        await using (var journal = new BinaryJournalService(_path, false))
        {
            await journal.AppendAsync(Entry(1));
        }

        await using var reopened = new BinaryJournalService(_path, false);
        Assert.Single(await reopened.ReadAllAsync());
    }

    [Fact]
    public async Task ReadAll_CorruptTail_DiscardsTrailingGarbage()
    {
        await using (var journal = new BinaryJournalService(_path, false))
        {
            await journal.AppendAsync(Entry(1));
        }

        // Append raw garbage that cannot be a valid framed record.
        await File.AppendAllTextAsync(_path, "garbage-not-a-record");

        await using var reopened = new BinaryJournalService(_path, false);
        var all = (await reopened.ReadAllAsync()).ToArray();

        Assert.Single(all);
        Assert.Equal(1, all[0].SequenceId);
    }

    [Fact]
    public async Task Reset_ClearsAllEntries()
    {
        await using var journal = new BinaryJournalService(_path, false);
        await journal.AppendAsync(Entry(1));

        await journal.ResetAsync();

        Assert.Empty(await journal.ReadAllAsync());
    }

    [Fact]
    public async Task TrimThroughSequence_RemovesEntriesUpToAndIncluding()
    {
        await using var journal = new BinaryJournalService(_path, false);
        await journal.AppendBatchAsync([Entry(1), Entry(2), Entry(3)]);

        await journal.TrimThroughSequenceAsync(2);
        var all = (await journal.ReadAllAsync()).ToArray();

        Assert.Single(all);
        Assert.Equal(3, all[0].SequenceId);
    }

    private static JournalEntry Entry(long seq)
    {
        return new JournalEntry
        {
            SequenceId = seq,
            TimestampUnixMilliseconds = seq * 10,
            TypeId = 1,
            Operation = JournalEntityOperationType.Upsert,
            Payload = [(byte)seq]
        };
    }
}
