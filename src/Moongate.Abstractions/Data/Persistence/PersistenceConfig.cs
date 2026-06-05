namespace Moongate.Abstractions.Data.Persistence;

/// <summary>
/// Configuration for the persistence service: autosave cadence and snapshot/journal file names.
/// </summary>
public sealed class PersistenceConfig
{
    /// <summary>How often a full snapshot is written and the journal trimmed. Default 300 s.</summary>
    public TimeSpan AutosaveInterval { get; set; } = TimeSpan.FromSeconds(300);

    /// <summary>Snapshot file name (under the Save directory).</summary>
    public string SnapshotFileName { get; set; } = "world.snapshot.bin";

    /// <summary>Journal file name (under the Save directory).</summary>
    public string JournalFileName { get; set; } = "world.journal.bin";

    /// <summary>When true, the journal file is opened with a per-process lock.</summary>
    public bool EnableFileLock { get; set; } = true;
}
