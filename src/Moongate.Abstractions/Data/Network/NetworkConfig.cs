namespace Moongate.Abstractions.Data.Network;

/// <summary>
///     Configuration for the network service: TCP game listener, UDP ping echo server and parser limits.
/// </summary>
public sealed class NetworkConfig
{
    /// <summary>
    ///     TCP port the game server listens on across every local interface. Default 2593 (UO).
    /// </summary>
    public int Port { get; set; } = 2593;

    /// <summary>
    ///     When <c>true</c>, starts the UDP ping echo server used by UO launchers to measure latency.
    /// </summary>
    public bool PingServerEnabled { get; set; } = true;

    /// <summary>
    ///     UDP port for the ping echo server. Default 12000.
    /// </summary>
    public int PingServerPort { get; set; } = 12000;

    /// <summary>
    ///     Maximum buffered unparsed bytes per session before the connection is dropped. Default 64 KB.
    /// </summary>
    public int MaxPendingBufferBytes { get; set; } = 64 * 1024;

    /// <summary>
    ///     Maximum length a variable-length packet may declare before it is treated as a protocol
    ///     violation. Default 16 KB.
    /// </summary>
    public int MaxDeclaredPacketLength { get; set; } = 16 * 1024;

    /// <summary>
    ///     Maximum number of pending client data items drained from the ingress queue per loop wake-up.
    /// </summary>
    public int MaxPacketsPerDrain { get; set; } = 256;

    /// <summary>
    ///     Maximum number of queued outbound packets drained per outbound loop wake-up.
    /// </summary>
    public int MaxOutgoingPacketsPerDrain { get; set; } = 256;
}
