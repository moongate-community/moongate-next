using System.Collections.Concurrent;
using System.Reflection;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Network;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Network;

/// <summary>
///     Dispatches parsed packet events to typed packet handlers registered in DI.
/// </summary>
public sealed class PacketDispatchHandler : ITickEventHandler<PacketReceivedEvent>
{
    private static readonly MethodInfo _dispatchMethod = typeof(PacketDispatchHandler).GetMethod(
        nameof(Dispatch),
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;

    private readonly ConcurrentDictionary<Type, Action<PacketDispatchHandler, PacketReceivedEvent>> _dispatchers = new();

    private readonly ILogger _logger = Log.ForContext<PacketDispatchHandler>();
    private readonly IOutgoingPacketQueue _outgoingPackets;
    private readonly IServiceProvider _serviceProvider;
    private readonly INetworkSessionManager _sessions;

    public PacketDispatchHandler(
        IServiceProvider serviceProvider,
        IOutgoingPacketQueue outgoingPackets,
        INetworkSessionManager sessions
    )
    {
        _serviceProvider = serviceProvider;
        _outgoingPackets = outgoingPackets;
        _sessions = sessions;
    }

    public void Handle(PacketReceivedEvent evt)
    {
        ArgumentNullException.ThrowIfNull(evt);

        var dispatcher = _dispatchers.GetOrAdd(evt.Packet.GetType(), CreateDispatcher);
        dispatcher(this, evt);
    }

    private static Action<PacketDispatchHandler, PacketReceivedEvent> CreateDispatcher(Type packetType)
    {
        var closedMethod = _dispatchMethod.MakeGenericMethod(packetType);

        return (handler, evt) => closedMethod.Invoke(handler, [evt]);
    }

    private void Dispatch<TPacket>(PacketReceivedEvent evt)
        where TPacket : IGameNetworkPacket
    {
        if (!_sessions.TryGetSession(evt.SessionId, out var session))
        {
            // The session disconnected between parse and dispatch; its packets are moot.
            _logger.Debug(
                "Dropping {Packet} for unknown session {SessionId}",
                typeof(TPacket).Name,
                evt.SessionId
            );

            return;
        }

        var handlers = _serviceProvider.GetService<IEnumerable<IPacketHandler<TPacket>>>() ?? [];

        var context = new PacketContext<TPacket>(
            session,
            (TPacket)evt.Packet,
            evt.At,
            EnqueuePacketAsync,
            GetSessionIds
        );

        foreach (var handler in handlers)
        {
            try
            {
                handler.HandleAsync(context).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Packet handler {Handler} failed for {Packet}",
                    handler.GetType().Name,
                    typeof(TPacket).Name
                );
            }
        }
    }

    private Task EnqueuePacketAsync(
        long sessionId,
        IGameNetworkPacket packet,
        CancellationToken cancellationToken
    )
    {
        _ = cancellationToken;
        _outgoingPackets.Enqueue(sessionId, packet);

        return Task.CompletedTask;
    }

    private IReadOnlyCollection<long> GetSessionIds()
    {
        return _sessions.GetSessionIds();
    }
}
