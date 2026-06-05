using Moongate.Abstractions.Types.Expansions;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.Login;

/// <summary>
/// Represents a supported client features packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Variable, Description = "Enable Locked Client Features")]
public class SupportFeaturesPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xB9;
    private const int ExtendedLength = 5;
    private const int LegacyLength = 3;

    public FeatureFlags Flags { get; set; }
    public bool UseExtendedFormat { get; set; }

    public SupportFeaturesPacket()
        : this(FeatureFlags.ExpansionEj, true) { }

    public SupportFeaturesPacket(FeatureFlags flags, bool useExtendedFormat)
        : base(OpCodeValue, useExtendedFormat ? ExtendedLength : LegacyLength)
    {
        Flags = flags;
        UseExtendedFormat = useExtendedFormat;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);

        if (UseExtendedFormat)
        {
            writer.Write((uint)Flags);

            return;
        }

        writer.Write((ushort)Flags);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining == 4)
        {
            Flags = (FeatureFlags)reader.ReadUInt32();
            UseExtendedFormat = true;

            return true;
        }

        if (reader.Remaining == 2)
        {
            Flags = (FeatureFlags)reader.ReadUInt16();
            UseExtendedFormat = false;

            return true;
        }

        return false;
    }
}
