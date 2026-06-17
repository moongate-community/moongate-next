using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Network.UO.Packets.Outgoing.Entity;

/// <summary>
///     Outgoing "Draw Game Player" (0x20): renders the player's own mobile in the world.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Draw Game Player")]
public class DrawPlayerPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x20;
    private const int LengthValue = 19;

    public DrawPlayerPacket(MobileEntity mobile)
        : base(OpCodeValue, LengthValue)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        Mobile = mobile;
    }

    public MobileEntity Mobile { get; }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Mobile.Id.Value);
        writer.Write((short)Mobile.BodyId);
        writer.Write((byte)0);
        writer.Write(Mobile.SkinHue.Value);
        writer.Write((byte)0);
        writer.Write((ushort)Mobile.Location.X);
        writer.Write((ushort)Mobile.Location.Y);
        writer.Write((ushort)0);
        writer.Write((byte)Mobile.Direction);
        writer.Write((sbyte)Mobile.Location.Z);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return false;
    }
}
