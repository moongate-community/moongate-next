namespace Moongate.UO.Data.Data.Internal;

/// <summary>TOML root binding for <c>bodies.toml</c>.</summary>
public sealed class BodyTableModel
{
    public BodyGroups Bodies { get; set; } = new();
}
