using Moongate.Network.UO.Packets.Incoming.Login;
using Moongate.Network.UO.Registry;

namespace Moongate.Tests.Network.Packets;

public sealed class LoginSeedPacketTests
{
    [Fact]
    public void PacketTable_Register_IncludesLoginSeedPacket()
    {
        var registry = new PacketRegistry();

        PacketTable.Register(registry);

        Assert.True(registry.TryGetDescriptor(0xEF, out var descriptor));
        Assert.Equal(21, descriptor.Length);
        Assert.Equal("KR/2D Client Login/Seed", descriptor.Description);
        Assert.Equal(typeof(LoginSeedPacket), descriptor.HandlerType);
        Assert.True(registry.TryCreatePacket(0xEF, out var packet));
        Assert.IsType<LoginSeedPacket>(packet);
    }

    [Fact]
    public void TryParse_ReadsSeedAndClientVersion()
    {
        var packet = new LoginSeedPacket();
        var raw = BuildLoginSeedPacket(0x12345678, 7, 0, 114, 0);

        var parsed = packet.TryParse(raw);

        Assert.True(parsed);
        Assert.Equal(0x12345678, packet.Seed);
        Assert.Equal(new(7, 0, 114, 0), packet.ClientVersion);
    }

    private static byte[] BuildLoginSeedPacket(int seed, int major, int minor, int revision, int patch)
    {
        var payload = new byte[21];
        payload[0] = 0xEF;
        WriteInt32(payload, 1, seed);
        WriteInt32(payload, 5, major);
        WriteInt32(payload, 9, minor);
        WriteInt32(payload, 13, revision);
        WriteInt32(payload, 17, patch);

        return payload;
    }

    private static void WriteInt32(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }
}
