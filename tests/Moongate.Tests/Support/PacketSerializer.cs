using Moongate.Network.Spans;
using Moongate.Network.UO.Base;

namespace Moongate.Tests.Support;

/// <summary>
/// Serializes an outgoing packet to its on-the-wire bytes for assertions in tests.
/// </summary>
public static class PacketSerializer
{
    public static byte[] Serialize(BaseGameNetworkPacket packet)
    {
        var writer = new SpanWriter(256, true);
        packet.Write(ref writer);

        return writer.Span.ToArray();
    }
}
