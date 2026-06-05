using Moongate.Network.Client;

namespace Moongate.Network.Events;

/// <summary>
/// Event payload containing a network client instance.
/// </summary>
public sealed class MoongateTCPClientEventArgs : EventArgs
{
    public MoongateTCPClientEventArgs(MoongateTCPClient client)
    {
        Client = client;
    }

    /// <summary>
    /// Connected or disconnected client.
    /// </summary>
    public MoongateTCPClient Client { get; }
}
