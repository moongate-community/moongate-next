using System.Text;
using Moongate.Network.UO.Packets.Incoming.Login;

namespace Moongate.Tests.Network.Packets;

public sealed class PlayCharacterPacketTests
{
    [Fact]
    public void ParsePayload_ReadsSlotAndName()
    {
        var raw = BuildPlayCharacter(2, "Tom");

        var packet = new PlayCharacterPacket();
        var parsed = packet.TryParse(raw);

        Assert.True(parsed);
        Assert.Equal(2, packet.Slot);
        Assert.Equal("Tom", packet.CharacterName);
    }

    [Fact]
    public void TryParse_WrongLength_ReturnsFalse()
    {
        var raw = new byte[50];
        raw[0] = 0x5D;

        var packet = new PlayCharacterPacket();

        Assert.False(packet.TryParse(raw));
    }

    private static byte[] BuildPlayCharacter(int slot, string name)
    {
        var buffer = new byte[73];
        buffer[0] = 0x5D;
        WriteInt32(buffer, 1, unchecked((int)0xEDEDEDED));
        var nameBytes = Encoding.ASCII.GetBytes(name);
        Array.Copy(nameBytes, 0, buffer, 5, nameBytes.Length); // 30-byte ascii at 5..34
        WriteInt32(buffer, 65, slot);

        return buffer;
    }

    private static void WriteInt32(byte[] b, int o, int v)
    {
        b[o] = (byte)(v >> 24);
        b[o + 1] = (byte)(v >> 16);
        b[o + 2] = (byte)(v >> 8);
        b[o + 3] = (byte)v;
    }
}
