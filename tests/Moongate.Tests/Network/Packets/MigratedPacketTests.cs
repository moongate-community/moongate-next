using System.Net;
using Moongate.Core.Ids;
using Moongate.Network.Spans;
using Moongate.Network.UO.Data.Login;
using Moongate.Network.UO.Packets.Incoming.UI;
using Moongate.Network.UO.Packets.Outgoing.Login;
using Moongate.Network.UO.Packets.Outgoing.Movement;
using Moongate.Network.UO.Registry;
using Moongate.Network.UO.Types.Login;

namespace Moongate.Tests.Network.Packets;

public sealed class MigratedPacketTests
{
    [Fact]
    public void GumpMenuSelectionPacket_TryParse_ReadsSerialAsStrongType()
    {
        var packet = new GumpMenuSelectionPacket();
        byte[] raw =
        [
            0xB1,
            0x00, 0x17,
            0x40, 0x00, 0x00, 0x01,
            0x00, 0x00, 0x12, 0x34,
            0x00, 0x00, 0x00, 0x2A,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        ];

        var parsed = packet.TryParse(raw);

        Assert.True(parsed);
        Assert.Equal((Serial)0x40000001u, packet.Serial);
        Assert.Equal(0x1234, packet.GumpId);
        Assert.Equal(0x2A, packet.ButtonId);
    }

    [Fact]
    public void LoginDeniedPacket_Write_EmitsReason()
    {
        var packet = new LoginDeniedPacket(LoginDeniedReasonType.BadPassword);

        var bytes = WritePacket(packet);

        Assert.Equal([0x82, 0x03], bytes);
    }

    [Fact]
    public void MoveConfirmPacket_Write_EmitsSequenceAndNotoriety()
    {
        var packet = new MoveConfirmPacket(0x10, 0x05);

        var bytes = WritePacket(packet);

        Assert.Equal([0x22, 0x10, 0x05], bytes);
    }

    [Theory, InlineData(0x03), InlineData(0x02), InlineData(0x21), InlineData(0x22), InlineData(0x55), InlineData(0x73),
     InlineData(0xAD), InlineData(0xB1), InlineData(0xBF), InlineData(0xC7), InlineData(0xF2)]
    public void PacketTable_Register_IncludesMigratedPackets(byte opCode)
    {
        var registry = new PacketRegistry();

        PacketTable.Register(registry);

        Assert.True(registry.TryGetDescriptor(opCode, out _));
        Assert.True(registry.TryCreatePacket(opCode, out var packet));
        Assert.Equal(opCode, packet?.OpCode);
    }

    [Fact]
    public void ServerListPacket_Write_EmitsShardEntries()
    {
        const string ServerName = "Moongate";
        var packet = new ServerListPacket(
            new GameServerEntry
            {
                Index = 1,
                ServerName = ServerName,
                IpAddress = IPAddress.Parse("127.0.0.1")
            }
        );

        var bytes = WritePacket(packet);

        Assert.Equal(0xA8, bytes[0]);
        Assert.Equal(46, (bytes[1] << 8) | bytes[2]);
        Assert.Equal(0x5D, bytes[3]);
        Assert.Equal(1, (bytes[4] << 8) | bytes[5]);
        Assert.Equal(1, (bytes[6] << 8) | bytes[7]);
        Assert.Equal((byte)ServerName[0], bytes[8]);
    }

    private static byte[] WritePacket(LoginDeniedPacket packet)
    {
        var writer = new SpanWriter(packet.Length, true);

        try
        {
            packet.Write(ref writer);

            return writer.ToArray();
        }
        finally
        {
            writer.Dispose();
        }
    }

    private static byte[] WritePacket(MoveConfirmPacket packet)
    {
        var writer = new SpanWriter(packet.Length, true);

        try
        {
            packet.Write(ref writer);

            return writer.ToArray();
        }
        finally
        {
            writer.Dispose();
        }
    }

    private static byte[] WritePacket(ServerListPacket packet)
    {
        var writer = new SpanWriter(64, true);

        try
        {
            packet.Write(ref writer);

            return writer.ToArray();
        }
        finally
        {
            writer.Dispose();
        }
    }
}
