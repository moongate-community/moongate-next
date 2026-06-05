using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Internal.Packets;
using Moongate.Network.UO.Types.GeneralInformation;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.GeneralInformation;

/// <summary>
/// Represents a general information packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Variable, Description = "General Information Packet")]
public class GeneralInformationPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xBF;
    private const int HeaderLength = 5;

    public GeneralInformationSubcommandType SubcommandType { get; set; }
    public ReadOnlyMemory<byte> SubcommandData { get; set; } = ReadOnlyMemory<byte>.Empty;

    public GeneralInformationPacket()
        : base(OpCodeValue) { }

    public GeneralInformationPacket(
        GeneralInformationSubcommandType subcommandType,
        ReadOnlyMemory<byte> subcommandData
    )
        : this()
    {
        SubcommandType = subcommandType;
        SubcommandData = subcommandData;
    }

    public override void Write(ref SpanWriter writer)
    {
        if (!GeneralInformationSubcommandRules.IsValid(SubcommandType, SubcommandData.Span))
        {
            throw new InvalidOperationException($"Invalid 0xBF payload for subcommand 0x{(ushort)SubcommandType:X2}.");
        }

        writer.Write(OpCode);
        writer.Write((ushort)(HeaderLength + SubcommandData.Length));
        writer.Write((ushort)SubcommandType);

        if (!SubcommandData.IsEmpty)
        {
            writer.Write(SubcommandData.Span);
        }
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (!PacketLengthValidator.TryReadVariableLength(ref reader) || reader.Length < HeaderLength)
        {
            return false;
        }

        SubcommandType = (GeneralInformationSubcommandType)reader.ReadUInt16();
        var dataLength = reader.Remaining;
        SubcommandData = dataLength == 0 ? ReadOnlyMemory<byte>.Empty : reader.ReadBytes(dataLength);

        return GeneralInformationSubcommandRules.IsValid(SubcommandType, SubcommandData.Span);
    }
}
