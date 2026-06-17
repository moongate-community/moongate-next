using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Login;

[PacketHandler(0x80, PacketSizing.Fixed, Length = 62, Description = "Login Request")]
/// <summary>
/// Represents AccountLoginPacket.
/// </summary>
public class AccountLoginPacket : BaseGameNetworkPacket
{
    public AccountLoginPacket()
        : base(0x80, 62)
    {
    }

    public string Account { get; set; }
    public string Password { get; set; }

    public byte NextLoginKey { get; set; }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        Account = reader.ReadAscii(30);
        Password = reader.ReadAscii(30);
        NextLoginKey = reader.ReadByte();

        return true;
    }
}
