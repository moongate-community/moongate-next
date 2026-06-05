using Moongate.UO.Data.Data.Expansions;

namespace Moongate.UO.Data.Data.Internal;

/// <summary>TOML root binding for <c>expansions.toml</c> (array of tables <c>[[expansion]]</c>).</summary>
public sealed class ExpansionTableModel
{
    public List<ExpansionInfo> Expansion { get; set; } = [];
}
