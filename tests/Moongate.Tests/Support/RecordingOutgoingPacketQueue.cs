using Moongate.Abstractions.Interfaces.Network;
using Moongate.Server.Data.Network;
using Moongate.Server.Interfaces.Network;

namespace Moongate.Tests.Support;

/// <summary>Captures enqueued packets per session for assertions.</summary>
public sealed class RecordingOutgoingPacketQueue : IOutgoingPacketQueue
{
    public List<(long SessionId, IGameNetworkPacket Packet)> Sent { get; } = [];

    public int Count => Sent.Count;

    public void Enqueue<TPacket>(long sessionId, TPacket packet)
        where TPacket : IGameNetworkPacket
        => Sent.Add((sessionId, packet));

    public int Clear(Action<OutgoingPacketEnvelope>? handler = null)
    {
        var n = Sent.Count;
        Sent.Clear();
        return n;
    }

    public int Drain(int maxItems, Func<OutgoingPacketEnvelope, bool> handler) => 0;
}
