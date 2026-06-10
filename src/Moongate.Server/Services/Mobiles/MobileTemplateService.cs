using System.Diagnostics.CodeAnalysis;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.Server.Services.Mobiles;

/// <summary>
/// Default in-memory mobile template registry keyed by case-insensitive id.
/// </summary>
public sealed class MobileTemplateService : IMobileTemplateService
{
    private readonly Dictionary<string, MobileTemplateDefinition> _templates = new(StringComparer.OrdinalIgnoreCase);

    public int Count => _templates.Count;

    public void Clear()
        => _templates.Clear();

    public IReadOnlyCollection<MobileTemplateDefinition> GetAll()
        => _templates.Values.ToArray();

    public bool TryGet(string id, [NotNullWhen(true)] out MobileTemplateDefinition? definition)
        => _templates.TryGetValue(id, out definition);

    public void UpsertRange(IEnumerable<MobileTemplateDefinition> templates)
    {
        ArgumentNullException.ThrowIfNull(templates);

        foreach (var template in templates)
        {
            _templates[template.Id] = template;
        }
    }
}
