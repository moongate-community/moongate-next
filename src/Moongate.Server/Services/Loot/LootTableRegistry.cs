using System.Diagnostics.CodeAnalysis;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Loot;

namespace Moongate.Server.Services.Loot;

/// <summary>
/// Immutable boot-time snapshot: loot tables by case-insensitive id plus an
/// index of concrete item templates by tag (for category resolution).
/// </summary>
public sealed class LootTableRegistry
{
    private readonly Dictionary<string, LootTableDefinition> _byId;
    private readonly Dictionary<string, IReadOnlyList<ItemTemplateDefinition>> _byTag;

    public int Count => _byId.Count;

    public LootTableRegistry(
        IEnumerable<LootTableDefinition> tables,
        IEnumerable<ItemTemplateDefinition> templates
    )
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(templates);

        _byId = new Dictionary<string, LootTableDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var table in tables)
        {
            _byId[table.Id] = table;
        }

        var byTag = new Dictionary<string, List<ItemTemplateDefinition>>(StringComparer.OrdinalIgnoreCase);

        foreach (var template in templates)
        {
            if (template.IsAbstract)
            {
                continue;
            }

            foreach (var tag in template.Tags)
            {
                if (!byTag.TryGetValue(tag, out var list))
                {
                    list = [];
                    byTag[tag] = list;
                }

                list.Add(template);
            }
        }

        _byTag = byTag.ToDictionary(
            static pair => pair.Key,
            static pair => (IReadOnlyList<ItemTemplateDefinition>)pair.Value,
            StringComparer.OrdinalIgnoreCase
        );
    }

    public bool TryGet(string id, [NotNullWhen(true)] out LootTableDefinition? table)
        => _byId.TryGetValue(id, out table);

    public bool TryGetByTag(string tag, [NotNullWhen(true)] out IReadOnlyList<ItemTemplateDefinition>? templates)
        => _byTag.TryGetValue(tag, out templates);
}
