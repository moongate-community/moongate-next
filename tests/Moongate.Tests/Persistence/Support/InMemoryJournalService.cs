using Moongate.Persistence.Data;
using Moongate.Persistence.Interfaces.Persistence;

namespace Moongate.Tests.Persistence.Support;

internal sealed class InMemoryJournalService : IJournalService
{
    public List<JournalEntry> Entries { get; } = [];

    public ValueTask AppendAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        Entries.Add(entry);

        return ValueTask.CompletedTask;
    }

    public ValueTask AppendBatchAsync(IReadOnlyList<JournalEntry> entries, CancellationToken cancellationToken = default)
    {
        Entries.AddRange(entries);

        return ValueTask.CompletedTask;
    }

    public ValueTask<IReadOnlyCollection<JournalEntry>> ReadAllAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult<IReadOnlyCollection<JournalEntry>>(Entries.ToArray());

    public ValueTask ResetAsync(CancellationToken cancellationToken = default)
    {
        Entries.Clear();

        return ValueTask.CompletedTask;
    }

    public ValueTask TrimThroughSequenceAsync(long inclusiveSequenceId, CancellationToken cancellationToken = default)
    {
        Entries.RemoveAll(e => e.SequenceId <= inclusiveSequenceId);

        return ValueTask.CompletedTask;
    }
}
