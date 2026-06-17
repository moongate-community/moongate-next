using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Network.UO.Packets.Outgoing.Entity;

/// <summary>
///     Outgoing "Paperdoll" (0x88): opens the character's paperdoll window with a display name.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Paperdoll")]
public class PaperdollPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x88;
    private const int LengthValue = 66;
    private const byte AllowLiftFlag = 0x02;

    public PaperdollPacket(MobileEntity mobile, string displayName)
        : base(OpCodeValue, LengthValue)
    {
        ArgumentNullException.ThrowIfNull(mobile);
        ArgumentNullException.ThrowIfNull(displayName);

        Mobile = mobile;
        DisplayName = displayName;
    }

    public MobileEntity Mobile { get; }
    public string DisplayName { get; }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Mobile.Id.Value);
        writer.WriteAscii(DisplayName, 60);
        writer.Write(AllowLiftFlag);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return false;
    }
}
