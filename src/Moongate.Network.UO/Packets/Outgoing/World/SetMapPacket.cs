using Moongate.Network.Spans;
using Moongate.Network.UO.Base;

namespace Moongate.Network.UO.Packets.Outgoing.World;

/// <summary>
/// Outgoing 0xBF general-information set-map (subcommand 0x08): selects the client's active map facet.
/// </summary>
public class SetMapPacket : BaseGameNetworkPacket
{
    private const ushort SetMapSubcommand = 0x0008;

    public int MapId { get; }

    public SetMapPacket(int mapId)
        : base(0xBF)
    {
        MapId = mapId;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((ushort)6);
        writer.Write(SetMapSubcommand);
        writer.Write((byte)MapId);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => false;
}
