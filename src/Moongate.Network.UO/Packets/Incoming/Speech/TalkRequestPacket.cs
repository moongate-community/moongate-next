using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Internal.Packets;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Speech;

/// <summary>
///     Represents an ASCII talk request packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Variable, Description = "Talk Request")]
public class TalkRequestPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x03;
    private const int MinimumPayloadLength = 5;

    public TalkRequestPacket()
        : base(OpCodeValue)
    {
    }

    public byte MessageType { get; private set; }
    public ushort Hue { get; private set; }
    public ushort Font { get; private set; }
    public string Text { get; private set; } = "";

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (!PacketLengthValidator.TryReadVariableLength(ref reader))
        {
            return false;
        }

        if (reader.Remaining < MinimumPayloadLength)
        {
            return false;
        }

        MessageType = reader.ReadByte();
        Hue = reader.ReadUInt16();
        Font = reader.ReadUInt16();
        Text = reader.ReadAsciiSafe().Trim();

        return Text.Length > 0;
    }
}
