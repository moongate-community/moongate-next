using System.Collections.Concurrent;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Server.Data.Network;
using Moongate.Server.Interfaces.Network;

namespace Moongate.Server.Services.Network;

/// <summary>
/// FIFO queue for outbound game packets.
/// </summary>
public sealed class OutgoingPacketQueue : IOutgoingPacketQueue
{
    private readonly ConcurrentQueue<OutgoingPacketEnvelope> _packets = new();

    public int Count => _packets.Count;

    public int Clear(Action<OutgoingPacketEnvelope>? handler = null)
    {
        var cleared = 0;

        while (_packets.TryDequeue(out var envelope))
        {
            cleared++;
            handler?.Invoke(envelope);
        }

        return cleared;
    }

    public int Drain(int maxItems, Func<OutgoingPacketEnvelope, bool> handler)
    {
        if (maxItems <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxItems), maxItems, "Drain budget must be positive.");
        }

        ArgumentNullException.ThrowIfNull(handler);

        var drained = 0;

        while (drained < maxItems && _packets.TryDequeue(out var envelope))
        {
            drained++;
            handler(envelope);
        }

        return drained;
    }

    public void Enqueue<TPacket>(long sessionId, TPacket packet)
        where TPacket : IGameNetworkPacket
    {
        ArgumentNullException.ThrowIfNull(packet);

        _packets.Enqueue(new(sessionId, packet, DateTimeOffset.UtcNow));
    }
}
