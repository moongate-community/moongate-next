using Moongate.Network.Client;

namespace Moongate.Network.Events;

/// <summary>
///     Event payload containing data received from a network client.
/// </summary>
public sealed class MoongateTCPDataReceivedEventArgs : EventArgs
{
    public MoongateTCPDataReceivedEventArgs(MoongateTCPClient client, ReadOnlyMemory<byte> data)
    {
        Client = client;
        Data = data;
    }

    /// <summary>
    ///     Source client for the data payload.
    /// </summary>
    public MoongateTCPClient Client { get; }

    /// <summary>
    ///     Received data payload.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }
}
