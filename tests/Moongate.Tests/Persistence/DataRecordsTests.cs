using MessagePack;
using MessagePack.Resolvers;
using Moongate.Persistence.Data;
using Moongate.Persistence.Types;

namespace Moongate.Tests.Persistence;

public class DataRecordsTests
{
    private static readonly MessagePackSerializerOptions _options =
        ContractlessStandardResolver.Options;

    [Fact]
    public void JournalEntry_RoundTrips()
    {
        var entry = new JournalEntry
        {
            SequenceId = 42,
            TimestampUnixMilliseconds = 1700,
            TypeId = 7,
            Operation = JournalEntityOperationType.Upsert,
            Payload = [1, 2, 3]
        };

        var bytes = MessagePackSerializer.Serialize(entry, _options);
        var back = MessagePackSerializer.Deserialize<JournalEntry>(bytes, _options);

        Assert.Equal(42, back.SequenceId);
        Assert.Equal(7, back.TypeId);
        Assert.Equal(JournalEntityOperationType.Upsert, back.Operation);
        Assert.Equal(new byte[] { 1, 2, 3 }, back.Payload);
    }
}
