using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Network.UO.Packets.Outgoing.Entity;

/// <summary>
/// Outgoing "Draw Game Player" (0x20): renders the player's own mobile in the world.
/// </summary>
[PacketHandler(0x20, PacketSizing.Fixed, Length = 19, Description = "Draw Game Player")]
public class DrawPlayerPacket : BaseGameNetworkPacket
{
    public MobileEntity Mobile { get; }

    public DrawPlayerPacket(MobileEntity mobile)
        : base(0x20, 19)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        Mobile = mobile;
    }

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
        => false;
}
