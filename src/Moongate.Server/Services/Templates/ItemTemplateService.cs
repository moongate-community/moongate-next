using System.Diagnostics.CodeAnalysis;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.Server.Services.Templates;

/// <summary>
///     Default in-memory item template registry keyed by case-insensitive template id.
/// </summary>
public sealed class ItemTemplateService : IItemTemplateService
{
    private readonly object _gate = new();

    private IReadOnlyDictionary<string, ItemTemplateDefinition> _templates =
        new Dictionary<string, ItemTemplateDefinition>(StringComparer.OrdinalIgnoreCase);

    public int Count => _templates.Count;

    public void Clear()
    {
        ReplaceAll([]);
    }

    public IReadOnlyCollection<ItemTemplateDefinition> GetAll()
    {
        return _templates.Values.ToArray();
    }

    public void ReplaceAll(IEnumerable<ItemTemplateDefinition> templates)
    {
        ArgumentNullException.ThrowIfNull(templates);

        lock (_gate)
        {
            _templates = templates.ToDictionary(
                static template => template.Id,
                static template => template,
                StringComparer.OrdinalIgnoreCase
            );
        }
    }

    public bool TryGet(string id, [NotNullWhen(true)] out ItemTemplateDefinition? definition)
    {
        return _templates.TryGetValue(id, out definition);
    }

    public void UpsertRange(IEnumerable<ItemTemplateDefinition> templates)
    {
        ArgumentNullException.ThrowIfNull(templates);

        lock (_gate)
        {
            var next = new Dictionary<string, ItemTemplateDefinition>(_templates, StringComparer.OrdinalIgnoreCase);

            foreach (var template in templates)
            {
                next[template.Id] = template;
            }

            _templates = next;
        }
    }
}
