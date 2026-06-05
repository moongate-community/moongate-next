using Moongate.Network.Spans;

namespace Moongate.Network.UO.Internal.Packets;

internal static class PacketLengthValidator
{
    public static bool TryReadVariableLength(ref SpanReader reader)
    {
        if (reader.Remaining < 2)
        {
            return false;
        }

        var declaredLength = reader.ReadUInt16();

        return declaredLength == reader.Length;
    }
}
