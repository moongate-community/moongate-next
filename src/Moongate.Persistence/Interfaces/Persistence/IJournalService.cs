using Moongate.Persistence.Data;

namespace Moongate.Persistence.Interfaces.Persistence;

/// <summary>
///     Appends and replays journal entries from durable storage.
/// </summary>
public interface IJournalService
{
    /// <summary>Appends one journal entry.</summary>
    ValueTask AppendAsync(JournalEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Appends multiple journal entries in one batched write.</summary>
    ValueTask AppendBatchAsync(IReadOnlyList<JournalEntry> entries, CancellationToken cancellationToken = default);

    /// <summary>Reads all valid journal entries in persistence order.</summary>
    ValueTask<IReadOnlyCollection<JournalEntry>> ReadAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Clears all journal content.</summary>
    ValueTask ResetAsync(CancellationToken cancellationToken = default);

    /// <summary>Removes journal entries up to and including the given sequence id.</summary>
    ValueTask TrimThroughSequenceAsync(long inclusiveSequenceId, CancellationToken cancellationToken = default);
}
