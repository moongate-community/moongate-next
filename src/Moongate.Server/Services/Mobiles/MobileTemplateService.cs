using System.Diagnostics.CodeAnalysis;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.Server.Services.Mobiles;

/// <summary>
/// Default in-memory mobile template registry keyed by case-insensitive id.
/// </summary>
public sealed class MobileTemplateService : IMobileTemplateService
{
    private readonly object _gate = new();

    private IReadOnlyDictionary<string, MobileTemplateDefinition> _templates =
        new Dictionary<string, MobileTemplateDefinition>(StringComparer.OrdinalIgnoreCase);

    public int Count => _templates.Count;

    public void Clear()
        => ReplaceAll([]);

    public IReadOnlyCollection<MobileTemplateDefinition> GetAll()
        => _templates.Values.ToArray();

    public void ReplaceAll(IEnumerable<MobileTemplateDefinition> templates)
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

    public bool TryGet(string id, [NotNullWhen(true)] out MobileTemplateDefinition? definition)
        => _templates.TryGetValue(id, out definition);

    public void UpsertRange(IEnumerable<MobileTemplateDefinition> templates)
    {
        ArgumentNullException.ThrowIfNull(templates);

        lock (_gate)
        {
            var next = new Dictionary<string, MobileTemplateDefinition>(_templates, StringComparer.OrdinalIgnoreCase);

            foreach (var template in templates)
            {
                next[template.Id] = template;
            }

            _templates = next;
        }
    }
}
