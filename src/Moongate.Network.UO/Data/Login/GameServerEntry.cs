using System.Net;
using Moongate.Core.Extensions.Network;
using Moongate.Network.Spans;

namespace Moongate.Network.UO.Data.Login;

/// <summary>
/// Represents a shard entry in the UO server list packet.
/// </summary>
public class GameServerEntry
{
    private const int NameLength = 32;

    public int Index { get; set; }
    public string ServerName { get; set; } = "";
    public IPAddress IpAddress { get; set; } = IPAddress.Loopback;

    public ReadOnlyMemory<byte> Write()
    {
        using var writer = new SpanWriter(40, true);

        writer.Write((short)Index);
        writer.WriteAscii(ServerName, NameLength);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write(IpAddress.ToRawAddress());

        return writer.ToArray().AsMemory();
    }
}
