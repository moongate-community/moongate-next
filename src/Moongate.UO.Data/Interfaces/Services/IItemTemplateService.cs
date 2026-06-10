using System.Diagnostics.CodeAnalysis;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.UO.Data.Interfaces.Services;

/// <summary>
/// In-memory registry of resolved item templates keyed by case-insensitive id.
/// Populated at boot by the item template boot service.
/// </summary>
public interface IItemTemplateService
{
    /// <summary>Number of registered templates.</summary>
    int Count { get; }

    /// <summary>Removes all registered templates.</summary>
    void Clear();

    /// <summary>Returns all registered templates.</summary>
    IReadOnlyCollection<ItemTemplateDefinition> GetAll();

    /// <summary>Gets a template by id; returns false when absent.</summary>
    bool TryGet(string id, [NotNullWhen(true)] out ItemTemplateDefinition? definition);

    /// <summary>Adds or replaces templates by id.</summary>
    void UpsertRange(IEnumerable<ItemTemplateDefinition> templates);
}
