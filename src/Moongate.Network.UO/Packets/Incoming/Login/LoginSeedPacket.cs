using Moongate.Abstractions.Data.Version;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Login;

[PacketHandler(0xEF, PacketSizing.Fixed, Length = 21, Description = "KR/2D Client Login/Seed")]
/// <summary>
/// Represents LoginSeedPacket.
/// </summary>
public class LoginSeedPacket : BaseGameNetworkPacket
{
    public LoginSeedPacket()
        : base(0xEF, 21)
    {
    }

    public int Seed { get; set; }
    public ClientVersion ClientVersion { get; set; }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        Seed = reader.ReadInt32();
        ClientVersion = new ClientVersion(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

        return true;
    }
}
