using Moongate.Abstractions.Interfaces.Network;

namespace Moongate.Abstractions.Data.Network;

/// <summary>
/// Context passed to typed packet handlers.
/// </summary>
/// <typeparam name="TPacket">Concrete inbound packet type.</typeparam>
public sealed class PacketContext<TPacket>
    where TPacket : IGameNetworkPacket
{
    private readonly Func<long, IGameNetworkPacket, CancellationToken, Task> _send;
    private readonly Func<IReadOnlyCollection<long>> _sessionIds;

    public PacketContext(
        long sessionId,
        TPacket packet,
        DateTimeOffset receivedAt,
        Func<long, IGameNetworkPacket, CancellationToken, Task> send,
        Func<IReadOnlyCollection<long>> sessionIds
    )
    {
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentNullException.ThrowIfNull(send);
        ArgumentNullException.ThrowIfNull(sessionIds);

        SessionId = sessionId;
        Packet = packet;
        ReceivedAt = receivedAt;
        _send = send;
        _sessionIds = sessionIds;
    }

    /// <summary>
    /// Session that sent the inbound packet.
    /// </summary>
    public long SessionId { get; }

    /// <summary>
    /// Parsed inbound packet.
    /// </summary>
    public TPacket Packet { get; }

    /// <summary>
    /// Timestamp captured when the packet was received.
    /// </summary>
    public DateTimeOffset ReceivedAt { get; }

    /// <summary>
    /// Queues an outbound packet for every active session, including the current session.
    /// </summary>
    public Task BroadcastAsync<TOutgoingPacket>(
        TOutgoingPacket packet,
        CancellationToken cancellationToken = default
    )
        where TOutgoingPacket : IGameNetworkPacket
    {
        ArgumentNullException.ThrowIfNull(packet);

        return SendToSessionsAsync(packet, _sessionIds(), cancellationToken);
    }

    /// <summary>
    /// Queues an outbound packet for every active session except the current session.
    /// </summary>
    public Task BroadcastExceptSelfAsync<TOutgoingPacket>(
        TOutgoingPacket packet,
        CancellationToken cancellationToken = default
    )
        where TOutgoingPacket : IGameNetworkPacket
    {
        ArgumentNullException.ThrowIfNull(packet);

        var targetSessionIds = _sessionIds().Where(sessionId => sessionId != SessionId).ToArray();

        return SendToSessionsAsync(packet, targetSessionIds, cancellationToken);
    }

    /// <summary>
    /// Queues an outbound packet for the current session.
    /// </summary>
    public Task SendAsync<TOutgoingPacket>(
        TOutgoingPacket packet,
        CancellationToken cancellationToken = default
    )
        where TOutgoingPacket : IGameNetworkPacket
    {
        ArgumentNullException.ThrowIfNull(packet);

        return _send(SessionId, packet, cancellationToken);
    }

    private async Task SendToSessionsAsync<TOutgoingPacket>(
        TOutgoingPacket packet,
        IEnumerable<long> sessionIds,
        CancellationToken cancellationToken
    )
        where TOutgoingPacket : IGameNetworkPacket
    {
        foreach (var sessionId in sessionIds)
        {
            await _send(sessionId, packet, cancellationToken);
        }
    }
}
