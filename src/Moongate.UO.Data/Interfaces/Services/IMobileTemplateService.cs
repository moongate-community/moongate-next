using System.Diagnostics.CodeAnalysis;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.UO.Data.Interfaces.Services;

/// <summary>
///     In-memory registry of resolved mobile templates keyed by case-insensitive id.
///     Populated at boot by the mobile template boot service.
/// </summary>
public interface IMobileTemplateService
{
    /// <summary>Number of registered templates.</summary>
    int Count { get; }

    /// <summary>Removes all registered templates.</summary>
    void Clear();

    /// <summary>Returns all registered templates.</summary>
    IReadOnlyCollection<MobileTemplateDefinition> GetAll();

    /// <summary>
    ///     Atomically replaces the entire registry with <paramref name="templates" />.
    ///     Equivalent to a clear followed by <see cref="UpsertRange" />, but performed as a
    ///     single locked swap so concurrent readers never observe an empty registry.
    /// </summary>
    void ReplaceAll(IEnumerable<MobileTemplateDefinition> templates);

    /// <summary>Gets a template by id; returns false when absent.</summary>
    bool TryGet(string id, [NotNullWhen(true)] out MobileTemplateDefinition? definition);

    /// <summary>Adds or replaces templates by id.</summary>
    void UpsertRange(IEnumerable<MobileTemplateDefinition> templates);
}
