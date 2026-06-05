using System.Collections.Concurrent;
using System.Net;
using System.Text;
using Moongate.Abstractions.Data.Logging;
using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Types.Metrics;
using Moongate.Core.Utils;
using Moongate.Network.Events;
using Moongate.Network.Server;
using Moongate.Network.UO.Registry;
using Moongate.Server.Data.Events;
using Moongate.Server.Data.Network;
using Moongate.Server.Interfaces.Network;
using Moongate.Server.Services.Network.Internal;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Network;

/// <summary>
/// Owns the TCP game listeners (one per local interface), the UDP ping echo server and the
/// background ingress thread that parses inbound bytes into packets and republishes them as
/// tick events on the event bus.
/// </summary>
public sealed class NetworkService : INetworkService, IMetricProvider, IDisposable
{
    private const int IngressIdleWaitMs = 5;
    private const int OutboundIdleWaitMs = 5;

    private readonly ILogger _logger = Log.ForContext<NetworkService>();
    private readonly IEventBusService _eventBus;
    private readonly ISessionService _sessions;
    private readonly IOutgoingPacketQueue _outgoingPackets;
    private readonly NetworkConfig _config;
    private readonly LoggerConfig _loggerConfig;
    private readonly PacketParser _parser;

    private readonly List<MoongateTCPServer> _tcpServers = [];
    private readonly ConcurrentQueue<PendingClientData> _pendingClientDataQueue = new();
    private readonly ConcurrentDictionary<long, NetworkParserSessionMetrics> _parserMetrics = new();
    private readonly AutoResetEvent _pendingClientDataSignal = new(false);

    private MoongateUDPServer? _pingServer;
    private Thread? _ingressThread;
    private Thread? _outboundThread;
    private volatile bool _ingressStopRequested;
    private volatile bool _outboundStopRequested;
    private long _ingressQueueDepth;
    private long _sentPackets;
    private long _droppedOutgoingPackets;
    private long _outgoingSendErrors;

    public NetworkService(
        IEventBusService eventBus,
        ISessionService sessions,
        IOutgoingPacketQueue outgoingPackets,
        PacketRegistry packetRegistry,
        NetworkConfig config,
        LoggerConfig? loggerConfig = null
    )
    {
        _eventBus = eventBus;
        _sessions = sessions;
        _outgoingPackets = outgoingPackets;
        _config = config;
        _loggerConfig = loggerConfig ?? new();
        _parser = new(packetRegistry, config.MaxPendingBufferBytes, config.MaxDeclaredPacketLength);
    }

    public int ConnectedSessionCount => _sessions.Count;

    public string Prefix => "network";

    private readonly struct PendingClientData
    {
        public long SessionId { get; }
        public byte[] Data { get; }

        public PendingClientData(long sessionId, byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);

            SessionId = sessionId;
            Data = data;
        }
    }

    public IReadOnlyList<MetricSample> Collect()
    {
        long receivedBytes = 0;
        long parsedPackets = 0;
        long unknownOpcodeDrops = 0;
        long parserErrors = 0;

        foreach (var metrics in _parserMetrics.Values)
        {
            receivedBytes += metrics.ReceivedBytes;
            parsedPackets += metrics.ParsedPackets;
            unknownOpcodeDrops += metrics.UnknownOpcodeDrops;
            parserErrors += metrics.UnknownOpcodeDrops +
                            metrics.InvalidLengthDrops +
                            metrics.ParseFailures +
                            metrics.PendingBufferOverflows;
        }

        return
        [
            new(
                "active_sessions",
                _sessions.Count,
                Help: "Currently connected sessions"
            ),
            new(
                "ingress_queue_depth",
                Interlocked.Read(ref _ingressQueueDepth),
                Help: "Pending client data items awaiting parsing"
            ),
            new(
                "received_bytes_total",
                receivedBytes,
                MetricType.Counter,
                Help: "Total bytes received across sessions"
            ),
            new(
                "parsed_packets_total",
                parsedPackets,
                MetricType.Counter,
                Help: "Total packets parsed across sessions"
            ),
            new(
                "unknown_opcode_drops_total",
                unknownOpcodeDrops,
                MetricType.Counter,
                Help: "Total bytes dropped for unknown opcodes"
            ),
            new(
                "parser_errors_total",
                parserErrors,
                MetricType.Counter,
                Help: "Total parser errors across sessions"
            ),
            new(
                "outgoing_queue_depth",
                _outgoingPackets.Count,
                Help: "Pending outbound packets awaiting delivery"
            ),
            new(
                "sent_packets_total",
                Interlocked.Read(ref _sentPackets),
                MetricType.Counter,
                Help: "Total outbound packets sent"
            ),
            new(
                "dropped_outgoing_packets_total",
                Interlocked.Read(ref _droppedOutgoingPackets),
                MetricType.Counter,
                Help: "Total outbound packets dropped before send"
            ),
            new(
                "outgoing_send_errors_total",
                Interlocked.Read(ref _outgoingSendErrors),
                MetricType.Counter,
                Help: "Total outbound packet send errors"
            )
        ];
    }

    public void Dispose()
    {
        StopIngressLoop();
        StopOutboundLoop();
        _pendingClientDataSignal.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartIngressLoop();
        StartOutboundLoop();
        StartPingServer(cancellationToken);
        StartTcpServers(cancellationToken);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        for (var i = _tcpServers.Count - 1; i >= 0; i--)
        {
            await _tcpServers[i].StopAsync(cancellationToken);
            await _tcpServers[i].DisposeAsync();
        }

        _tcpServers.Clear();

        if (_pingServer is not null)
        {
            await _pingServer.StopAsync(cancellationToken);
            await _pingServer.DisposeAsync();
            _pingServer = null;
        }

        StopIngressLoop();
        StopOutboundLoop();

        _sessions.Clear();
        _parserMetrics.Clear();

        while (_pendingClientDataQueue.TryDequeue(out _))
        {
            Interlocked.Decrement(ref _ingressQueueDepth);
        }

        _outgoingPackets.Clear(static envelope => DisposePacket(envelope.Packet));
    }

    private static string BuildHexDump(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return "<empty>";
        }

        var builder = new StringBuilder((data.Length / 16 + 1) * 80);

        for (var i = 0; i < data.Length; i += 16)
        {
            var lineLength = Math.Min(16, data.Length - i);
            builder.Append(i.ToString("X4"));
            builder.Append("  ");

            for (var j = 0; j < 16; j++)
            {
                if (j < lineLength)
                {
                    builder.Append(data[i + j].ToString("X2"));
                }
                else
                {
                    builder.Append("  ");
                }

                if (j != 15)
                {
                    builder.Append(' ');
                }
            }

            builder.Append("  |");

            for (var j = 0; j < lineLength; j++)
            {
                var value = data[i + j];
                builder.Append(value is >= 32 and <= 126 ? (char)value : '.');
            }

            builder.Append('|');

            if (i + lineLength < data.Length)
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    private static void DisposePacket(IGameNetworkPacket packet)
    {
        if (packet is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private void LogOutgoingPacket(OutgoingPacketEnvelope envelope, byte[] payload)
        => _logger.Information(
            ">> packet Session={SessionId} OpCode=0x{OpCode:X2} Name={PacketName} Length={Length}{NewLine}{Dump}",
            envelope.SessionId,
            envelope.Packet.OpCode,
            envelope.Packet.GetType().Name,
            payload.Length,
            Environment.NewLine,
            BuildHexDump(payload)
        );

    private void OnClientConnected(object? sender, MoongateTCPClientEventArgs e)
    {
        var session = _sessions.GetOrCreate(e.Client);
        _parserMetrics.TryAdd(session.SessionId, new());

        _logger.Information(
            "Client connected. SessionId={SessionId}, RemoteEndPoint={RemoteEndPoint}",
            session.SessionId,
            e.Client.RemoteEndPoint
        );
    }

    private void OnClientData(object? sender, MoongateTCPDataReceivedEventArgs e)
    {
        if (e.Data.IsEmpty)
        {
            return;
        }

        _pendingClientDataQueue.Enqueue(new(e.Client.SessionId, e.Data.ToArray()));
        Interlocked.Increment(ref _ingressQueueDepth);
        _pendingClientDataSignal.Set();
    }

    private void OnClientDisconnected(object? sender, MoongateTCPClientEventArgs e)
    {
        var remoteEndPoint = e.Client.RemoteEndPoint?.ToString();
        _sessions.Remove(e.Client.SessionId);
        _parserMetrics.TryRemove(e.Client.SessionId, out _);

        _logger.Information(
            "Client disconnected. SessionId={SessionId}, RemoteEndPoint={RemoteEndPoint}",
            e.Client.SessionId,
            remoteEndPoint
        );
    }

    private void OnClientException(object? sender, MoongateTCPExceptionEventArgs e)
        => _logger.Error(e.Exception, "Client network exception");

    private void ProcessClientData(long sessionId, byte[] data)
    {
        if (!_sessions.TryGet(sessionId, out var session))
        {
            return;
        }

        var metrics = _parserMetrics.GetOrAdd(sessionId, static _ => new());

        session.WithPendingBytes(
            pendingBytes => _parser.Append(
                pendingBytes,
                data,
                metrics,
                (opCode, packet, rawPacket) =>
                {
                    if (_loggerConfig.LogPackets)
                    {
                        _logger.Information(
                            "<< packet Session={SessionId} OpCode=0x{OpCode:X2} Name={PacketName} Length={Length}{NewLine}{Dump}",
                            session.SessionId,
                            opCode,
                            packet.GetType().Name,
                            rawPacket.Length,
                            Environment.NewLine,
                            BuildHexDump(rawPacket)
                        );
                    }

                    _eventBus.Publish(new PacketReceivedEvent(session.SessionId, opCode, packet, DateTimeOffset.UtcNow));
                }
            )
        );
    }

    private void RunIngressLoop()
    {
        while (!_ingressStopRequested)
        {
            var processed = 0;

            while (processed < _config.MaxPacketsPerDrain && _pendingClientDataQueue.TryDequeue(out var pending))
            {
                Interlocked.Decrement(ref _ingressQueueDepth);
                processed++;

                try
                {
                    ProcessClientData(pending.SessionId, pending.Data);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unhandled exception in network ingress loop");
                }
            }

            if (processed == 0)
            {
                _pendingClientDataSignal.WaitOne(IngressIdleWaitMs);
            }
        }
    }

    private void RunOutboundLoop()
    {
        var maxPacketsPerDrain = Math.Max(1, _config.MaxOutgoingPacketsPerDrain);

        while (!_outboundStopRequested)
        {
            var processed = _outgoingPackets.Drain(maxPacketsPerDrain, SendQueuedPacket);

            if (processed == 0)
            {
                Thread.Sleep(OutboundIdleWaitMs);
            }
        }
    }

    private bool SendQueuedPacket(OutgoingPacketEnvelope envelope)
    {
        if (!_sessions.TryGet(envelope.SessionId, out var session))
        {
            DisposePacket(envelope.Packet);
            Interlocked.Increment(ref _droppedOutgoingPackets);

            return true;
        }

        try
        {
            if (_loggerConfig.LogPackets)
            {
                session.SendPacket(envelope.Packet, payload => LogOutgoingPacket(envelope, payload))
                       .GetAwaiter()
                       .GetResult();
            }
            else
            {
                session.SendPacket(envelope.Packet).GetAwaiter().GetResult();
            }

            Interlocked.Increment(ref _sentPackets);
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _outgoingSendErrors);
            _logger.Error(ex, "Unhandled exception in network outbound loop");
        }

        return true;
    }

    private void StartIngressLoop()
    {
        if (_ingressThread is not null)
        {
            return;
        }

        _ingressStopRequested = false;
        _ingressThread = new(RunIngressLoop)
        {
            IsBackground = true,
            Name = "Moongate-NetworkIngress"
        };
        _ingressThread.Start();
    }

    private void StartOutboundLoop()
    {
        if (_outboundThread is not null)
        {
            return;
        }

        _outboundStopRequested = false;
        _outboundThread = new(RunOutboundLoop)
        {
            IsBackground = true,
            Name = "Moongate-NetworkOutbound"
        };
        _outboundThread.Start();
    }

    private void StartPingServer(CancellationToken cancellationToken)
    {
        if (!_config.PingServerEnabled || _config.PingServerPort <= 0)
        {
            return;
        }

        _pingServer = new(new(IPAddress.Any, _config.PingServerPort));
        _ = _pingServer.StartAsync(cancellationToken);
    }

    private void StartTcpServers(CancellationToken cancellationToken)
    {
        foreach (var endPoint in NetworkUtils.GetListeningAddresses(new(IPAddress.Any, _config.Port)))
        {
            var server = new MoongateTCPServer(new(endPoint.Address, _config.Port));
            server.OnClientConnect += OnClientConnected;
            server.OnClientDisconnect += OnClientDisconnected;
            server.OnDataReceived += OnClientData;
            server.OnException += OnClientException;

            _tcpServers.Add(server);
            _ = server.StartAsync(cancellationToken);
            _logger.Information("TCP game server listening on {Address}:{Port}", endPoint.Address, _config.Port);
        }
    }

    private void StopIngressLoop()
    {
        if (_ingressThread is null)
        {
            return;
        }

        _ingressStopRequested = true;
        _pendingClientDataSignal.Set();
        _ingressThread.Join(TimeSpan.FromSeconds(2));
        _ingressThread = null;
    }

    private void StopOutboundLoop()
    {
        if (_outboundThread is null)
        {
            return;
        }

        _outboundStopRequested = true;
        _outboundThread.Join(TimeSpan.FromSeconds(2));
        _outboundThread = null;
    }
}
