using System.Buffers.Binary;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Internal.Packets;
using Moongate.Network.UO.Types.Login;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Login;

/// <summary>
///     Represents a KR or SA client type packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Variable, Description = "Client Type")]
public class ClientTypePacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xE1;

    public ClientTypePacket()
        : base(OpCodeValue)
    {
    }

    public uint AdvertisedClientType { get; private set; }
    public ClientType ResolvedClientType { get; private set; } = ClientType.Classic;
    public string VersionString { get; private set; } = "";

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (!PacketLengthValidator.TryReadVariableLength(ref reader))
        {
            return false;
        }

        var payloadLength = reader.Remaining;

        if (payloadLength < 4)
        {
            return false;
        }

        var payload = reader.ReadBytes(payloadLength);
        var payloadReader = new SpanReader(payload);

        try
        {
            if (!TryReadPayload(payload, payloadLength, ref payloadReader))
            {
                return false;
            }
        }
        finally
        {
            payloadReader.Dispose();
        }

        ResolvedClientType = AdvertisedClientType switch
        {
            0x02 => ClientType.KingdomReborn,
            0x03 => ClientType.StygianAbyss,
            _ => ClientType.Classic
        };

        return true;
    }

    private bool TryReadPayload(byte[] payload, int payloadLength, ref SpanReader payloadReader)
    {
        if (payloadLength == 4)
        {
            AdvertisedClientType = payloadReader.ReadUInt32();
            VersionString = "";

            return true;
        }

        var firstField = BinaryPrimitives.ReadUInt16BigEndian(payload);
        var secondField = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(2));

        if (secondField is 0x02 or 0x03)
        {
            _ = payloadReader.ReadUInt16();
            AdvertisedClientType = payloadReader.ReadUInt16();
            VersionString = payloadReader.ReadAscii(payloadLength - 4).TrimEnd('\0').Trim();

            return true;
        }

        if (firstField is 0x02 or 0x03)
        {
            AdvertisedClientType = payloadReader.ReadUInt16();
            VersionString = payloadReader.ReadAscii(payloadLength - 2).TrimEnd('\0').Trim();

            return true;
        }

        if (payloadLength == 6)
        {
            _ = payloadReader.ReadUInt16();
            AdvertisedClientType = payloadReader.ReadUInt32();
            VersionString = "";

            return true;
        }

        return false;
    }
}
