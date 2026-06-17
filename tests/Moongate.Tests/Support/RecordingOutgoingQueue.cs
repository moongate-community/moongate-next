using Moongate.Abstractions.Interfaces.Network;
using Moongate.Server.Data.Network;
using Moongate.Server.Interfaces.Network;

namespace Moongate.Tests.Support;

/// <summary>
///     IOutgoingPacketQueue stub that records every enqueued packet with its session id.
///     All other members throw <see cref="NotSupportedException" />.
/// </summary>
public sealed class RecordingOutgoingQueue : IOutgoingPacketQueue
{
    public List<(long SessionId, IGameNetworkPacket Packet)> Sent { get; } = [];

    public int Count => Sent.Count;

    public int Clear(Action<OutgoingPacketEnvelope>? handler = null)
    {
        throw new NotSupportedException();
    }

    public int Drain(int maxItems, Func<OutgoingPacketEnvelope, bool> handler)
    {
        throw new NotSupportedException();
    }

    public void Enqueue<TPacket>(long sessionId, TPacket packet)
        where TPacket : IGameNetworkPacket
    {
        Sent.Add((sessionId, packet));
    }
}
