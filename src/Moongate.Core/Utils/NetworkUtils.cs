using System.Net;
using System.Net.NetworkInformation;

namespace Moongate.Core.Utils;

/// <summary>
/// Provides utilities for enumerating the host's network interfaces and addresses.
/// </summary>
public static class NetworkUtils
{
    /// <summary>
    /// Enumerates one <see cref="IPEndPoint" /> per local unicast address that matches the
    /// address family of <paramref name="endPoint" />, preserving its port.
    /// </summary>
    /// <param name="endPoint">Template endpoint supplying the port and address family to match.</param>
    /// <returns>An endpoint for every matching unicast address across all network interfaces.</returns>
    public static IEnumerable<IPEndPoint> GetListeningAddresses(IPEndPoint endPoint)
    {
        ArgumentNullException.ThrowIfNull(endPoint);

        return NetworkInterface.GetAllNetworkInterfaces()
                               .SelectMany(
                                   adapter =>
                                       adapter.GetIPProperties()
                                              .UnicastAddresses
                                              .Where(unicast => endPoint.AddressFamily == unicast.Address.AddressFamily)
                                              .Select(unicast => new IPEndPoint(unicast.Address, endPoint.Port))
                               );
    }
}
