using Moongate.Network.UO.Packets.Incoming.Login;
using Xunit;

namespace Moongate.Tests.Network.Packets;

public sealed class PlayCharacterPacketTests
{
    [Fact]
    public void ParsePayload_ReadsSlotAndName()
    {
        var raw = BuildPlayCharacter(slot: 2, name: "Tom");

        var packet = new PlayCharacterPacket();
        var parsed = packet.TryParse(raw);

        Assert.True(parsed);
        Assert.Equal(2, packet.Slot);
        Assert.Equal("Tom", packet.CharacterName);
    }

    private static byte[] BuildPlayCharacter(int slot, string name)
    {
        var buffer = new byte[73];
        buffer[0] = 0x5D;
        WriteInt32(buffer, 1, unchecked((int)0xEDEDEDED));
        var nameBytes = System.Text.Encoding.ASCII.GetBytes(name);
        System.Array.Copy(nameBytes, 0, buffer, 5, nameBytes.Length); // 30-byte ascii at 5..34
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
