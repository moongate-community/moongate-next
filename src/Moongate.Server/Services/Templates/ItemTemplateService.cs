using System.Diagnostics.CodeAnalysis;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.Server.Services.Templates;

/// <summary>
/// Default in-memory item template registry keyed by case-insensitive template id.
/// </summary>
public sealed class ItemTemplateService : IItemTemplateService
{
    private readonly Dictionary<string, ItemTemplateDefinition> _templates = new(StringComparer.OrdinalIgnoreCase);

    public int Count => _templates.Count;

    public void Clear()
        => _templates.Clear();

    public IReadOnlyCollection<ItemTemplateDefinition> GetAll()
        => _templates.Values.ToArray();

    public bool TryGet(string id, [NotNullWhen(true)] out ItemTemplateDefinition? definition)
        => _templates.TryGetValue(id, out definition);

    public void UpsertRange(IEnumerable<ItemTemplateDefinition> templates)
    {
        ArgumentNullException.ThrowIfNull(templates);

        foreach (var template in templates)
        {
            _templates[template.Id] = template;
        }
    }
}
