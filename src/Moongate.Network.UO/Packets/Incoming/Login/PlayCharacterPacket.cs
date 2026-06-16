using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Login;

/// <summary>
/// Incoming "Play Character" (0x5D): the client requests to enter the world with a chosen character slot.
/// </summary>
[PacketHandler(0x5D, PacketSizing.Fixed, Length = 73, Description = "Play Character")]
public class PlayCharacterPacket : BaseGameNetworkPacket
{
    public string CharacterName { get; set; } = string.Empty;
    public int Slot { get; set; }

    public PlayCharacterPacket()
        : base(0x5D, 73) { }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(0);
        writer.WriteAscii(CharacterName, 30);
        writer.Write((ushort)0);
        writer.Write((uint)0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(new byte[16]);
        writer.Write(Slot);
        writer.Write((uint)0);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 72)
        {
            return false;
        }

        reader.ReadInt32();                 // pattern1
        CharacterName = reader.ReadAscii(30);
        reader.ReadUInt16();                // unknown0
        reader.ReadUInt32();                // client flags
        reader.ReadInt32();                 // unknown1
        reader.ReadInt32();                 // login count
        reader.ReadBytes(16);               // unknown2
        Slot = reader.ReadInt32();
        reader.ReadUInt32();                // client ip

        return reader.Remaining == 0;
    }
}
