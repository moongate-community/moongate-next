using System.Reflection;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Data.Internal.Packets;
using Moongate.Network.UO.Data.Packets;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Registry;

/// <summary>
///     Represents PacketRegistry.
/// </summary>
public class PacketRegistry
{
    private readonly Dictionary<byte, PacketRegistration> _registrations = [];

    public IReadOnlyList<PacketDescriptor> RegisteredPackets
        =>
        [
            .. _registrations.Values
                .Select(static registration => registration.Descriptor)
                .OrderBy(static descriptor => descriptor.OpCode)
        ];

    public void RegisterFixed<TPacket>(byte opcode, int length, string? description = null)
        where TPacket : IGameNetworkPacket, new()
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Fixed packet length must be greater than zero.");
        }

        Register<TPacket>(opcode, PacketSizing.Fixed, length, description);
    }

    /// <summary>
    ///     Scans <paramref name="assembly" /> for non-abstract <see cref="IGameNetworkPacket" /> types
    ///     annotated with <see cref="PacketHandlerAttribute" /> and registers each one.
    /// </summary>
    /// <param name="assembly">Assembly to scan for packet types.</param>
    /// <returns>The number of packets registered.</returns>
    public int RegisterFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var count = 0;

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || !type.IsClass || !typeof(IGameNetworkPacket).IsAssignableFrom(type))
            {
                continue;
            }

            var attribute = type.GetCustomAttributes(typeof(PacketHandlerAttribute), false)
                .OfType<PacketHandlerAttribute>()
                .SingleOrDefault();

            if (attribute is null)
            {
                continue;
            }

            RegisterFromType(type, attribute);
            count++;
        }

        return count;
    }

    public void RegisterFromAttribute<TPacket>()
        where TPacket : IGameNetworkPacket, new()
    {
        var attribute = typeof(TPacket).GetCustomAttributes(typeof(PacketHandlerAttribute), false)
            .OfType<PacketHandlerAttribute>()
            .SingleOrDefault();

        if (attribute is null)
        {
            throw new InvalidOperationException($"Packet type '{typeof(TPacket).Name}' is missing PacketHandlerAttribute.");
        }

        if (attribute.Sizing == PacketSizing.Fixed)
        {
            RegisterFixed<TPacket>(attribute.OpCode, attribute.Length, attribute.Description);

            return;
        }

        RegisterVariable<TPacket>(attribute.OpCode, attribute.Description);
    }

    public void RegisterVariable<TPacket>(byte opcode, string? description = null)
        where TPacket : IGameNetworkPacket, new()
    {
        Register<TPacket>(opcode, PacketSizing.Variable, -1, description);
    }

    public bool TryCreatePacket(byte opcode, out IGameNetworkPacket? packet)
    {
        if (_registrations.TryGetValue(opcode, out var registration))
        {
            packet = registration.Factory();

            return true;
        }

        packet = null;

        return false;
    }

    public bool TryGetDescriptor(byte opcode, out PacketDescriptor descriptor)
    {
        if (_registrations.TryGetValue(opcode, out var registration))
        {
            descriptor = registration.Descriptor;

            return true;
        }

        descriptor = default;

        return false;
    }

    private void Register<TPacket>(byte opcode, PacketSizing sizing, int length, string? description)
        where TPacket : IGameNetworkPacket, new()
    {
        if (_registrations.ContainsKey(opcode))
        {
            throw new InvalidOperationException($"Packet opcode 0x{opcode:X2} is already registered.");
        }

        var resolvedDescription = ResolveDescription(opcode, typeof(TPacket), description);
        var descriptor = new PacketDescriptor(opcode, sizing, length, resolvedDescription, typeof(TPacket));
        var registration = new PacketRegistration(descriptor, static () => new TPacket());
        _registrations.Add(opcode, registration);
    }

    private void RegisterFromType(Type packetType, PacketHandlerAttribute attribute)
    {
        var sizing = attribute.Sizing;
        var length = sizing == PacketSizing.Fixed ? attribute.Length : -1;

        if (sizing == PacketSizing.Fixed && length <= 0)
        {
            throw new InvalidOperationException($"Fixed packet '{packetType.Name}' must declare a positive Length.");
        }

        if (_registrations.ContainsKey(attribute.OpCode))
        {
            throw new InvalidOperationException($"Packet opcode 0x{attribute.OpCode:X2} is already registered.");
        }

        var resolvedDescription = ResolveDescription(attribute.OpCode, packetType, attribute.Description);
        var descriptor = new PacketDescriptor(attribute.OpCode, sizing, length, resolvedDescription, packetType);
        var registration = new PacketRegistration(
            descriptor,
            () => (IGameNetworkPacket)Activator.CreateInstance(packetType)!
        );
        _registrations.Add(attribute.OpCode, registration);
    }

    private static string ResolveDescription(byte opcode, Type packetType, string? explicitDescription)
    {
        _ = opcode;

        return !string.IsNullOrWhiteSpace(explicitDescription)
            ? explicitDescription
            : packetType.Name.Replace("Packet", "", StringComparison.Ordinal);
    }
}
