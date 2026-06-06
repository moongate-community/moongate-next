namespace Moongate.UO.Data.Data.Internal;

/// <summary>YAML root binding for <c>bodies.yaml</c>.</summary>
public sealed class BodyTableModel
{
    public BodyGroups Bodies { get; set; } = new();
}
