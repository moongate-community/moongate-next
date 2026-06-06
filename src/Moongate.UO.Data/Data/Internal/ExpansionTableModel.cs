using Moongate.UO.Data.Data.Expansions;

namespace Moongate.UO.Data.Data.Internal;

/// <summary>YAML root binding for <c>expansions.yaml</c>.</summary>
public sealed class ExpansionTableModel
{
    public List<ExpansionInfo> Expansion { get; set; } = [];
}
