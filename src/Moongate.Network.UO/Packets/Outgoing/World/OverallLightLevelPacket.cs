using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Environment;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.World;

/// <summary>
///     Represents a global light level packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Overall Light Level")]
public class OverallLightLevelPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x4F;
    private const int LengthValue = 2;

    public OverallLightLevelPacket()
        : base(OpCodeValue, LengthValue)
    {
    }

    public OverallLightLevelPacket(LightLevelType lightLevel)
        : this()
    {
        LightLevel = lightLevel;
    }

    public LightLevelType LightLevel { get; set; }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((byte)LightLevel);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 1)
        {
            return false;
        }

        LightLevel = (LightLevelType)reader.ReadByte();

        return reader.Remaining == 0;
    }
}
