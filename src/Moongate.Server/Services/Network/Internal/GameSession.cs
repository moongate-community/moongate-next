using Moongate.Abstractions.Interfaces.Network;
using Moongate.Network.Client;
using Moongate.Network.Spans;

namespace Moongate.Server.Services.Network.Internal;

/// <summary>
/// Minimal per-connection session: tracks the owning client and a pending byte buffer that the
/// network parser accumulates into until full packets can be extracted.
/// </summary>
public sealed class GameSession
{
    private const int DefaultPacketBufferCapacity = 256;

    private readonly Lock _pendingBytesSync = new();
    private readonly List<byte> _pendingBytes = [];

    /// <summary>
    /// Unique identifier of the session, sourced from the owning client.
    /// </summary>
    public long SessionId { get; }

    /// <summary>
    /// Owning TCP client.
    /// </summary>
    public MoongateTCPClient Client { get; }

    public GameSession(MoongateTCPClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        Client = client;
        SessionId = client.SessionId;
    }

    /// <summary>
    /// Serializes and sends a packet to the owning client.
    /// </summary>
    public Task SendPacket<TPacket>(TPacket packet, CancellationToken cancellationToken = default)
        where TPacket : IGameNetworkPacket
        => SendPacketAsync(packet, cancellationToken);

    /// <summary>
    /// Creates, serializes and sends a packet to the owning client.
    /// </summary>
    public Task SendPacket<TPacket>(CancellationToken cancellationToken = default)
        where TPacket : IGameNetworkPacket, new()
        => SendPacketAsync<TPacket>(cancellationToken);

    /// <summary>
    /// Serializes and sends a packet to the owning client.
    /// </summary>
    public Task SendPacketAsync<TPacket>(TPacket packet, CancellationToken cancellationToken = default)
        where TPacket : IGameNetworkPacket
        => SendPacketAsync(packet, null, cancellationToken);

    /// <summary>
    /// Creates, serializes and sends a packet to the owning client.
    /// </summary>
    public Task SendPacketAsync<TPacket>(CancellationToken cancellationToken = default)
        where TPacket : IGameNetworkPacket, new()
        => SendPacketAsync(new TPacket(), cancellationToken);

    /// <summary>
    /// Executes <paramref name="action" /> with exclusive access to the pending byte buffer.
    /// </summary>
    /// <param name="action">Action receiving the locked pending byte list.</param>
    public void WithPendingBytes(Action<List<byte>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        lock (_pendingBytesSync)
        {
            action(_pendingBytes);
        }
    }

    internal Task SendPacket<TPacket>(
        TPacket packet,
        Action<byte[]>? onSerialized,
        CancellationToken cancellationToken = default
    )
        where TPacket : IGameNetworkPacket
        => SendPacketAsync(packet, onSerialized, cancellationToken);

    internal Task SendPacketAsync<TPacket>(
        TPacket packet,
        Action<byte[]>? onSerialized,
        CancellationToken cancellationToken = default
    )
        where TPacket : IGameNetworkPacket
    {
        ArgumentNullException.ThrowIfNull(packet);

        try
        {
            var payload = SerializePacket(packet);
            onSerialized?.Invoke(payload);

            return Client.SendAsync(payload, cancellationToken);
        }
        finally
        {
            if (packet is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private static byte[] SerializePacket(IGameNetworkPacket packet)
    {
        var initialCapacity = packet.Length > 0 ? packet.Length : DefaultPacketBufferCapacity;
        var writer = new SpanWriter(initialCapacity, true);

        try
        {
            packet.Write(ref writer);

            return writer.ToArray();
        }
        finally
        {
            writer.Dispose();
        }
    }
}
