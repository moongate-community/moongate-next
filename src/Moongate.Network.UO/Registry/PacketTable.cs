namespace Moongate.Network.UO.Registry;

/// <summary>
/// Registers all known UO protocol packets.
/// Packet sizes from: https://docs.polserver.com/packets/
/// </summary>
public static class PacketTable
{
    /// <summary>
    /// Registers every packet type in this assembly that is annotated with
    /// <see cref="Attributes.PacketHandlerAttribute" /> into <paramref name="registry" />.
    /// </summary>
    /// <param name="registry">Registry to populate.</param>
    /// <returns>The number of packets registered.</returns>
    public static int Register(PacketRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        return registry.RegisterFromAssembly(typeof(PacketTable).Assembly);
    }
}
