namespace Moongate.Abstractions.Attributes;

/// <summary>
/// Opt-in marker: a packet-handler class carrying this attribute is auto-discovered at boot and
/// registered for every <c>IPacketHandler&lt;&gt;</c> interface it implements.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RegisterPacketHandlerAttribute : Attribute { }
