using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Environment;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.World;

/// <summary>
/// Represents a weather update packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Set Weather")]
public class SetWeatherPacket : BaseGameNetworkPacket
{
    public const byte MaximumEffectsOnScreen = 70;

    private const byte OpCodeValue = 0x65;
    private const int LengthValue = 4;

    public WeatherType Type { get; set; }
    public byte EffectCount { get; set; }
    public byte Temperature { get; set; }

    public SetWeatherPacket()
        : base(OpCodeValue, LengthValue) { }

    public SetWeatherPacket(WeatherType type, byte effectCount, byte temperature)
        : this()
    {
        Type = type;
        EffectCount = effectCount;
        Temperature = temperature;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((byte)Type);
        writer.Write(EffectCount);
        writer.Write(Temperature);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 3)
        {
            return false;
        }

        Type = (WeatherType)reader.ReadByte();
        EffectCount = reader.ReadByte();
        Temperature = reader.ReadByte();

        return reader.Remaining == 0;
    }
}
