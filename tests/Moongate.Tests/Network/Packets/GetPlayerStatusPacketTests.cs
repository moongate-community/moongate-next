using Moongate.Network.UO.Packets.Incoming.Player;
using Moongate.Network.UO.Types.Player;
using Xunit;

namespace Moongate.Tests.Network.Packets;

public sealed class GetPlayerStatusPacketTests
{
    [Fact]
    public void ParsePayload_ReadsStatusTypeAndSerial()
    {
        var raw = BuildGetPlayerStatus(GetPlayerStatusType.BasicStatus, serial: 0x00000001);

        var packet = new GetPlayerStatusPacket();
        var parsed = packet.TryParse(raw);

        Assert.True(parsed);
        Assert.Equal(GetPlayerStatusType.BasicStatus, packet.StatusType);
        Assert.Equal(0x00000001u, packet.MobileSerial);
    }

    [Fact]
    public void ParsePayload_WrongLength_ReturnsFalse()
    {
        var raw = new byte[6];
        raw[0] = 0x34;

        var packet = new GetPlayerStatusPacket();

        Assert.False(packet.TryParse(raw));
    }

    private static byte[] BuildGetPlayerStatus(GetPlayerStatusType type, uint serial)
    {
        var buffer = new byte[10];
        buffer[0] = 0x34;
        WriteUInt32(buffer, 1, 0xEDEDEDED); // pattern
        buffer[5] = (byte)type;
        WriteUInt32(buffer, 6, serial);

        return buffer;
    }

    private static void WriteUInt32(byte[] b, int o, uint v)
    {
        b[o] = (byte)(v >> 24);
        b[o + 1] = (byte)(v >> 16);
        b[o + 2] = (byte)(v >> 8);
        b[o + 3] = (byte)v;
    }
}
