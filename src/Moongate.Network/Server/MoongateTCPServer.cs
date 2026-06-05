using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Moongate.Network.Client;
using Moongate.Network.Events;
using Moongate.Network.Interfaces.Framing;
using Moongate.Network.Interfaces.Middleware;
using Serilog;

namespace Moongate.Network.Server;

/// <summary>
/// High-throughput TCP server with client lifecycle events and middleware-enabled payload dispatch.
/// Supports Start/Stop/Start cycles by recreating the underlying socket on each Start.
/// </summary>
public sealed class MoongateTCPServer : IAsyncDisposable, IDisposable
{
    private const int DefaultBacklog = 512;

    private readonly ILogger _logger = Log.ForContext<MoongateTCPServer>();
    private readonly Lock _middlewareSync = new();
    private readonly ConcurrentDictionary<long, MoongateTCPClient> _clients = new();
    private readonly IPEndPoint _endPoint;
    private readonly INetFramer? _framer;
    private readonly int _receiveBufferSize;
    private readonly int _historyBufferCapacity;

    private INetMiddleware[] _middlewares = [];
    private Socket? _serverSocket;
    private CancellationTokenSource? _listenerCancellationTokenSource;
    private Task? _acceptLoopTask;
    private int _started;

    /// <summary>
    /// Initializes a TCP server bound to the given endpoint.
    /// </summary>
    /// <param name="endPoint">Endpoint to bind on every <see cref="StartAsync" />.</param>
    /// <param name="framer">
    /// Optional framer template. The same instance is shared by all accepted clients,
    /// so implementations must be stateless or thread-safe.
    /// </param>
    /// <param name="receiveBufferSize">Per-client receive chunk size.</param>
    /// <param name="historyBufferCapacity">Per-client history buffer capacity.</param>
    public MoongateTCPServer(
        IPEndPoint endPoint,
        INetFramer? framer = null,
        int receiveBufferSize = 8192,
        int historyBufferCapacity = 65536
    )
    {
        _endPoint = endPoint;
        _framer = framer;
        _receiveBufferSize = receiveBufferSize;
        _historyBufferCapacity = historyBufferCapacity;
    }

    /// <summary>
    /// Raised when a client connects.
    /// </summary>
    public event EventHandler<MoongateTCPClientEventArgs>? OnClientConnect;

    /// <summary>
    /// Raised when a client disconnects.
    /// </summary>
    public event EventHandler<MoongateTCPClientEventArgs>? OnClientDisconnect;

    /// <summary>
    /// Raised when a client sends data after middleware processing.
    /// </summary>
    public event EventHandler<MoongateTCPDataReceivedEventArgs>? OnDataReceived;

    /// <summary>
    /// Raised when an exception happens in accept loop or client loops.
    /// </summary>
    public event EventHandler<MoongateTCPExceptionEventArgs>? OnException;

    /// <summary>
    /// Current listening port. Returns 0 when the server is stopped.
    /// </summary>
    public int Port => ((IPEndPoint?)_serverSocket?.LocalEndPoint)?.Port ?? 0;

    /// <summary>
    /// True when the server is currently accepting connections.
    /// </summary>
    public bool IsRunning => Volatile.Read(ref _started) != 0;

    /// <summary>
    /// Registers middleware in execution order.
    /// </summary>
    public MoongateTCPServer AddMiddleware(INetMiddleware middleware)
    {
        lock (_middlewareSync)
        {
            _middlewares = [.. _middlewares, middleware];
        }

        return this;
    }

    /// <inheritdoc />
    public void Dispose()
        => DisposeAsync().AsTask().GetAwaiter().GetResult();

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
        => await StopAsync(CancellationToken.None);

    /// <summary>
    /// Starts accepting clients. Recreates the listening socket on every call,
    /// so Stop/Start cycles are supported.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _started, 1) != 0)
        {
            return Task.CompletedTask;
        }

        _serverSocket = new(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(_endPoint);
        _serverSocket.Listen(DefaultBacklog);

        _listenerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _acceptLoopTask = Task.Run(AcceptLoopAsync, CancellationToken.None);

        _logger.Information("TCP server listening on {LocalEndPoint}", _serverSocket.LocalEndPoint);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops accepting new clients and closes all active clients.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _started, 0) == 0)
        {
            return;
        }

        if (_listenerCancellationTokenSource is not null)
        {
            await _listenerCancellationTokenSource.CancelAsync();
        }

        var socket = _serverSocket;

        try
        {
            socket?.Close();
        }
        catch (SocketException)
        {
            // Listener may already be closed.
        }

        if (_acceptLoopTask is not null)
        {
            try
            {
                await _acceptLoopTask.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Stop was cancelled by caller; clients are still cleaned up below.
            }
        }

        var clients = _clients.Values.ToArray();

        for (var i = 0; i < clients.Length; i++)
        {
            await clients[i].DisposeAsync();
        }

        _clients.Clear();

        socket?.Dispose();
        _serverSocket = null;

        _listenerCancellationTokenSource?.Dispose();
        _listenerCancellationTokenSource = null;
        _acceptLoopTask = null;
    }

    private async Task AcceptLoopAsync()
    {
        var cts = _listenerCancellationTokenSource;
        var serverSocket = _serverSocket;

        if (cts is null || serverSocket is null)
        {
            return;
        }

        while (!cts.IsCancellationRequested)
        {
            try
            {
                var clientSocket = await serverSocket.AcceptAsync(cts.Token);

                var middlewareSnapshot = _middlewares;
                var client = new MoongateTCPClient(
                    clientSocket,
                    middlewareSnapshot,
                    _framer,
                    _receiveBufferSize,
                    _historyBufferCapacity
                );
                WireClientEvents(client);

                _clients[client.SessionId] = client;
                await client.StartAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Accept loop failed");
                OnException?.Invoke(this, new(ex));
            }
        }
    }

    private void WireClientEvents(MoongateTCPClient client)
    {
        client.OnConnected += (_, args) =>
                              {
                                  _logger.Information(
                                      "OnClientConnect. SessionId={SessionId}, RemoteEndPoint={RemoteEndPoint}",
                                      args.Client.SessionId,
                                      args.Client.RemoteEndPoint
                                  );
                                  OnClientConnect?.Invoke(this, args);
                              };
        client.OnDataReceived += (_, args) =>
                                 {
                                     _logger.Verbose(
                                         "OnDataReceived. SessionId={SessionId}, Bytes={Bytes}",
                                         args.Client.SessionId,
                                         args.Data.Length
                                     );
                                     OnDataReceived?.Invoke(this, args);
                                 };
        client.OnException += (_, args) =>
                              {
                                  _logger.Error(
                                      args.Exception,
                                      "OnException. SessionId={SessionId}",
                                      args.Client?.SessionId
                                  );
                                  OnException?.Invoke(this, args);
                              };
        client.OnDisconnected += (_, args) =>
                                 {
                                     _clients.TryRemove(args.Client.SessionId, out var _);
                                     _logger.Information(
                                         "OnClientDisconnect. SessionId={SessionId}, RemoteEndPoint={RemoteEndPoint}",
                                         args.Client.SessionId,
                                         args.Client.RemoteEndPoint
                                     );
                                     OnClientDisconnect?.Invoke(this, args);
                                 };
    }
}
