using Moongate.Server.Interfaces.Services.World;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Loadouts;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Loadouts;

namespace Moongate.Server.Services.Loadouts;

/// <summary>
/// Boot-time fail-fast validation for the starter loadout definition against the
/// item template registry and the profession catalog. Any violation throws so the
/// server refuses to start with a broken loadout.
/// </summary>
public static class StarterLoadoutValidator
{
    private static readonly string[] ValidRaceKeys = ["human", "elf", "gargoyle"];

    public static void Validate(
        StarterLoadoutDefinition definition,
        string sourceFile,
        IItemTemplateService templates,
        IProfessionDataService professions
    )
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(professions);

        ValidateRaceKeys(definition, sourceFile);
        ValidateProfessionKeys(definition, sourceFile, professions);

        foreach (var (sectionName, section) in AllSections(definition))
        {
            ValidateSection(sectionName, section, sourceFile, templates);
        }

        ValidateBackpackTemplate(definition, sourceFile, templates);
        ValidateLayerConflicts(definition, sourceFile, templates);
    }

    private static IEnumerable<(string Name, LoadoutSection Section)> AllSections(StarterLoadoutDefinition definition)
    {
        yield return ("base", definition.Base);

        foreach (var (key, section) in definition.Races)
        {
            yield return ($"races/{key}", section);
        }

        foreach (var (key, section) in definition.Professions)
        {
            yield return ($"professions/{key}", section);
        }
    }

    private static void ValidateRaceKeys(StarterLoadoutDefinition definition, string sourceFile)
    {
        foreach (var key in definition.Races.Keys)
        {
            if (!ValidRaceKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Starter loadout '{sourceFile}' has unknown race key '{key}' (valid: human, elf, gargoyle)."
                );
            }
        }
    }

    private static void ValidateProfessionKeys(
        StarterLoadoutDefinition definition,
        string sourceFile,
        IProfessionDataService professions
    )
    {
        if (definition.Professions.Count == 0)
        {
            return;
        }

        var knownNames = new HashSet<string>(
            professions.GetAllProfessions().Select(static profession => profession.Name),
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var key in definition.Professions.Keys)
        {
            if (!knownNames.Contains(key))
            {
                throw new InvalidOperationException($"Starter loadout '{sourceFile}' has unknown profession key '{key}'.");
            }
        }
    }

    private static void ValidateSection(
        string sectionName,
        LoadoutSection section,
        string sourceFile,
        IItemTemplateService templates
    )
    {
        foreach (var entry in section.EquipItems)
        {
            var template = ResolveTemplate(entry, sectionName, sourceFile, templates);

            if (template.Layer is null)
            {
                throw new InvalidOperationException(
                    $"Starter loadout '{sourceFile}' section '{sectionName}' equips template '{template.Id}' which has no layer."
                );
            }
        }

        foreach (var entry in section.BackpackItems)
        {
            ResolveTemplate(entry, sectionName, sourceFile, templates);

            if (entry.PacketHue != PacketHueSource.None)
            {
                throw new InvalidOperationException(
                    $"Starter loadout '{sourceFile}' section '{sectionName}' backpack item '{entry.Template}' declares packet_hue (equip entries only)."
                );
            }
        }
    }

    private static ItemTemplateDefinition ResolveTemplate(
        LoadoutItemEntry entry,
        string sectionName,
        string sourceFile,
        IItemTemplateService templates
    )
    {
        if (string.IsNullOrWhiteSpace(entry.Template))
        {
            throw new InvalidOperationException(
                $"Starter loadout '{sourceFile}' section '{sectionName}' has an entry with an empty template id."
            );
        }

        if (!templates.TryGet(entry.Template, out var template))
        {
            throw new InvalidOperationException(
                $"Starter loadout '{sourceFile}' section '{sectionName}' references unknown item template '{entry.Template}'."
            );
        }

        if (template.IsAbstract)
        {
            throw new InvalidOperationException(
                $"Starter loadout '{sourceFile}' section '{sectionName}' references abstract item template '{entry.Template}'."
            );
        }

        if (entry.Amount is < 1)
        {
            throw new InvalidOperationException(
                $"Starter loadout '{sourceFile}' section '{sectionName}' entry '{entry.Template}' has invalid amount {entry.Amount}."
            );
        }

        return template;
    }

    private static void ValidateBackpackTemplate(
        StarterLoadoutDefinition definition,
        string sourceFile,
        IItemTemplateService templates
    )
    {
        var hasBackpackItems = AllSections(definition).Any(static pair => pair.Section.BackpackItems.Count > 0);

        if (string.IsNullOrWhiteSpace(definition.BackpackTemplate))
        {
            if (hasBackpackItems)
            {
                throw new InvalidOperationException(
                    $"Starter loadout '{sourceFile}' declares backpack items but no backpack_template."
                );
            }

            return;
        }

        if (!templates.TryGet(definition.BackpackTemplate, out var template))
        {
            throw new InvalidOperationException(
                $"Starter loadout '{sourceFile}' backpack_template references unknown item template '{definition.BackpackTemplate}'."
            );
        }

        if (template.IsAbstract)
        {
            throw new InvalidOperationException(
                $"Starter loadout '{sourceFile}' backpack_template references abstract item template '{definition.BackpackTemplate}'."
            );
        }

        if (template.Layer is null)
        {
            throw new InvalidOperationException(
                $"Starter loadout '{sourceFile}' backpack_template '{definition.BackpackTemplate}' has no layer (it gets equipped)."
            );
        }
    }

    private static void ValidateLayerConflicts(
        StarterLoadoutDefinition definition,
        string sourceFile,
        IItemTemplateService templates
    )
    {
        ItemLayerType? backpackLayer = null;

        if (!string.IsNullOrWhiteSpace(definition.BackpackTemplate) &&
            templates.TryGet(definition.BackpackTemplate, out var backpackTemplate))
        {
            backpackLayer = backpackTemplate.Layer;
        }

        foreach (var raceKey in definition.Races.Keys.Prepend(null))
        {
            foreach (var professionKey in definition.Professions.Keys.Prepend(null))
            {
                var seen = new Dictionary<ItemLayerType, string>();

                if (backpackLayer is { } layer)
                {
                    seen[layer] = $"backpack_template '{definition.BackpackTemplate}'";
                }

                CheckSectionLayers(definition.Base, "base", raceKey, professionKey, seen, sourceFile, templates);

                if (raceKey is not null)
                {
                    CheckSectionLayers(
                        definition.Races[raceKey],
                        $"races/{raceKey}",
                        raceKey,
                        professionKey,
                        seen,
                        sourceFile,
                        templates
                    );
                }

                if (professionKey is not null)
                {
                    CheckSectionLayers(
                        definition.Professions[professionKey],
                        $"professions/{professionKey}",
                        raceKey,
                        professionKey,
                        seen,
                        sourceFile,
                        templates
                    );
                }
            }
        }
    }

    private static void CheckSectionLayers(
        LoadoutSection section,
        string sectionName,
        string? raceKey,
        string? professionKey,
        Dictionary<ItemLayerType, string> seen,
        string sourceFile,
        IItemTemplateService templates
    )
    {
        foreach (var entry in section.EquipItems)
        {
            if (!templates.TryGet(entry.Template, out var template) || template.Layer is not { } entryLayer)
            {
                continue;
            }

            if (seen.TryGetValue(entryLayer, out var previous))
            {
                throw new InvalidOperationException(
                    $"Starter loadout '{sourceFile}' layer conflict on {entryLayer} for race '{raceKey ?? "-"}' + profession '{professionKey ?? "-"}': '{sectionName}/{entry.Template}' collides with {previous}."
                );
            }

            seen[entryLayer] = $"'{sectionName}/{entry.Template}'";
        }
    }
}
