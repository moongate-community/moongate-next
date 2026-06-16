using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Network.UO.Packets.Outgoing.Login;

/// <summary>
/// Outgoing "Login Confirm" (0x1B): tells the client its serial, body, location, direction and map size.
/// </summary>
[PacketHandler(0x1B, PacketSizing.Fixed, Length = 37, Description = "Char Locale and Body")]
public class LoginConfirmPacket : BaseGameNetworkPacket
{
    private const int LengthValue = 37;

    public MobileEntity Mobile { get; }
    public int MapWidth { get; }
    public int MapHeight { get; }

    public LoginConfirmPacket(MobileEntity mobile, int mapWidth, int mapHeight)
        : base(0x1B, LengthValue)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        Mobile = mobile;
        MapWidth = mapWidth;
        MapHeight = mapHeight;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Mobile.Id.Value);
        writer.Write(0);
        writer.Write((short)Mobile.BodyId);
        writer.Write((short)Mobile.Location.X);
        writer.Write((short)Mobile.Location.Y);
        writer.Write((short)Mobile.Location.Z);
        writer.Write((byte)Mobile.Direction);
        writer.Write((byte)0);
        writer.Write(-1);
        writer.Write(0);
        writer.Write((short)MapWidth);
        writer.Write((short)MapHeight);
        writer.Clear(LengthValue - writer.Position);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => false;
}
