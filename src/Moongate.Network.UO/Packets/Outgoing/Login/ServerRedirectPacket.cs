using System.Net;
using Moongate.Core.Extensions.Network;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.Login;

/// <summary>
/// Represents a game server redirect packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Connect To Game Server")]
public class ServerRedirectPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x8C;
    private const int LengthValue = 11;

    public IPAddress IpAddress { get; set; } = IPAddress.Loopback;
    public int Port { get; set; }
    public uint SessionKey { get; set; }

    public ServerRedirectPacket()
        : base(OpCodeValue, LengthValue) { }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.WriteLE(IpAddress.ToRawAddress());
        writer.Write((ushort)Port);
        writer.Write(SessionKey);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => reader.Remaining == LengthValue - 1;
}
