using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PacketHandlerAttribute : Attribute
{
    public byte OpCode { get; }
    public PacketSizing Sizing { get; }
    public int Length { get; init; } = -1;
    public string? Description { get; init; }

    public PacketHandlerAttribute(byte opCode, PacketSizing sizing)
    {
        OpCode = opCode;
        Sizing = sizing;
    }
}
