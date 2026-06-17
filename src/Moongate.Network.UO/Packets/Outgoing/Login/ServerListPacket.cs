using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Data.Login;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.Login;

/// <summary>
///     Represents a game server list packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Variable, Description = "Game Server List")]
public class ServerListPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xA8;
    private const int HeaderLength = 6;
    private const int ShardEntryLength = 40;

    public ServerListPacket()
        : base(OpCodeValue)
    {
    }

    public ServerListPacket(params GameServerEntry[] entries)
        : this()
    {
        if (entries.Length > 0)
        {
            Shards.AddRange(entries);
        }
    }

    public List<GameServerEntry> Shards { get; } = [];

    public void AddShard(GameServerEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        Shards.Add(entry);
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((ushort)(HeaderLength + ShardEntryLength * Shards.Count));
        writer.Write((byte)0x5D);
        writer.Write((ushort)Shards.Count);

        foreach (var shard in Shards)
        {
            writer.Write(shard.Write().Span);
        }
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return reader.Remaining >= 2;
    }
}
