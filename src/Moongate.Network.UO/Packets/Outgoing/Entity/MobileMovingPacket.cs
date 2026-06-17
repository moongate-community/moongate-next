using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Data;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Network.UO.Packets.Outgoing.Entity;

/// <summary>Outgoing "Mobile Moving" (0x77): updates a visible mobile's position, facing and hue.</summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Mobile Moving")]
public class MobileMovingPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x77;
    private const int LengthValue = 17;

    public MobileMovingPacket(MobileEntity mobile)
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
        writer.Write((ushort)Mobile.Location.X);
        writer.Write((ushort)Mobile.Location.Y);
        writer.Write((sbyte)Mobile.Location.Z);
        writer.Write((byte)Mobile.Direction);
        writer.Write(Mobile.SkinHue.Value);
        writer.Write(MobilePacketFlags.For(Mobile));
        writer.Write((byte)Mobile.Notoriety);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return false;
    }
}
