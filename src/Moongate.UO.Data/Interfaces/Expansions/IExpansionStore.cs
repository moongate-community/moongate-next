using Moongate.UO.Data.Data.Expansions;
using Moongate.UO.Data.Types.Expansions;

namespace Moongate.UO.Data.Interfaces.Expansions;

/// <summary>Provides the UO expansion capability table.</summary>
public interface IExpansionStore
{
    /// <summary>All loaded expansions.</summary>
    IReadOnlyList<ExpansionInfo> Table { get; }

    /// <summary>Number of loaded expansions.</summary>
    int Count { get; }

    /// <summary>The highest loaded expansion (the shard "core" expansion), or <c>null</c>.</summary>
    ExpansionInfo? Core { get; }

    /// <summary>Returns the expansion by id, or <c>null</c>.</summary>
    /// <param name="id">Expansion id.</param>
    ExpansionInfo? GetInfo(int id);

    /// <summary>Returns the expansion by era, or <c>null</c>.</summary>
    /// <param name="expansion">Expansion era.</param>
    ExpansionInfo? GetInfo(UoExpansionType expansion);
}
