using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Data.Packets;

/// <summary>
///     Describes a registered UO packet.
/// </summary>
public readonly record struct PacketDescriptor
{
    public PacketDescriptor(
        byte opCode,
        PacketSizing sizing,
        int length,
        string description,
        Type handlerType
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(handlerType);

        OpCode = opCode;
        Sizing = sizing;
        Length = length;
        Description = description;
        HandlerType = handlerType;
    }

    public byte OpCode { get; }
    public PacketSizing Sizing { get; }
    public int Length { get; }
    public string Description { get; }
    public Type HandlerType { get; }
}
