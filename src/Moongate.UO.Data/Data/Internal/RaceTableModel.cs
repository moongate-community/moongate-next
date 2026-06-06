using Moongate.UO.Data.Data.Races;

namespace Moongate.UO.Data.Data.Internal;

/// <summary>YAML root binding for <c>races.yaml</c>.</summary>
public sealed class RaceTableModel
{
    public List<RaceDefinition> Race { get; set; } = [];
}
