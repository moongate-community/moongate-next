using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types.Mobiles;

namespace Moongate.Network.UO.Packets.Outgoing.Entity;

/// <summary>
/// Outgoing "Status Bar Info" (0x11), MVP version-1 layout: name, vitals, base stats.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Variable, Description = "Status Bar Info")]
public class PlayerStatusPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x11;
    private const byte Version = 1;

    public MobileEntity Mobile { get; }

    public PlayerStatusPacket(MobileEntity mobile)
        : base(OpCodeValue)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        Mobile = mobile;
    }

    public override void Write(ref SpanWriter writer)
    {
        var maxHits = Mobile.Resources.MaxHits > 0 ? Mobile.Resources.MaxHits : Math.Max(1, Mobile.Resources.Hits);
        var currentHits = Math.Clamp(Mobile.Resources.Hits, 0, maxHits);

        writer.Write(OpCode);
        writer.Write((ushort)0);
        writer.Write(Mobile.Id.Value);
        writer.WriteAscii(Mobile.Name ?? string.Empty, 30);
        writer.WriteAttribute(maxHits, currentHits, true, true);
        writer.Write(false);
        writer.Write(Version);

        writer.Write(Mobile.Gender == GenderType.Female);
        writer.Write((ushort)Mobile.BaseStats.Strength);
        writer.Write((ushort)Mobile.BaseStats.Dexterity);
        writer.Write((ushort)Mobile.BaseStats.Intelligence);
        writer.Write((ushort)Mobile.Resources.Stamina);
        writer.Write((ushort)Mobile.Resources.MaxStamina);
        writer.Write((ushort)Mobile.Resources.Mana);
        writer.Write((ushort)Mobile.Resources.MaxMana);
        writer.Write((uint)0);
        writer.Write((ushort)0);
        writer.Write((ushort)0);

        writer.WritePacketLength();
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => false;
}
