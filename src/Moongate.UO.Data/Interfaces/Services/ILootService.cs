using Moongate.UO.Data.Entities.Items;

namespace Moongate.UO.Data.Interfaces.Services;

/// <summary>
/// Resolves named loot tables into persisted item entities.
/// </summary>
public interface ILootService
{
    /// <summary>
    /// Resolves the loot table into a list of persisted item entities.
    /// Throws when the loot table id is unknown.
    /// </summary>
    ValueTask<IReadOnlyList<ItemEntity>> GenerateAsync(string lootTableId, CancellationToken cancellationToken = default);

    /// <summary>True when a loot table with the given id is registered.</summary>
    bool Has(string lootTableId);
}
