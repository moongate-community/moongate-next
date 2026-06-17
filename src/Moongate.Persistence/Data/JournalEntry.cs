using Moongate.Persistence.Types;

namespace Moongate.Persistence.Data;

/// <summary>
///     Journal record appended for every persisted mutation.
/// </summary>
public sealed class JournalEntry
{
    public long SequenceId { get; set; }
    public long TimestampUnixMilliseconds { get; set; }
    public ushort TypeId { get; set; }
    public JournalEntityOperationType Operation { get; set; }
    public byte[] Payload { get; set; } = [];
}
