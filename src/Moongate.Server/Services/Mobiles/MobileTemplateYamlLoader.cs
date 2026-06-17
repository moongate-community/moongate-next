using Moongate.Core.Yaml;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Mobiles;
using Moongate.UO.Data.Types.Mobiles;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Mobiles;

/// <summary>
///     Loads mobile template YAML files, merging every file's <c>mobile_templates</c>
///     list and resolving the <c>base_mobile</c> inheritance chain. Parsing is strict
///     (fail fast with the file path); a missing/empty directory is a warning.
/// </summary>
/// <remarks>
///     Inheritance merge: scalar fields use default-value sentinels (a child at its
///     default inherits the parent); nullable blocks (stats/resources/resistances)
///     inherit whole when the child's is null; lists inherit when the child's is
///     empty; dictionaries (skills/params) merge key-by-key (child overrides). The
///     sentinel strategy means a child cannot explicitly re-state a default over a
///     non-default parent value — e.g. <c>gender: Male</c> (the zero member) over a
///     <c>Female</c> parent, or <c>notoriety: Innocent</c> over a <c>Criminal</c>
///     parent, both inherit the parent. Deliberate KISS tradeoff; switch the DTO
///     fields to nullables if explicit overrides become necessary. Inherited stats/
///     resources/resistances blocks are shared (treated as immutable template data).
/// </remarks>
public sealed class MobileTemplateYamlLoader
{
    private readonly ILogger _logger = Log.ForContext<MobileTemplateYamlLoader>();
    private readonly string _mobilesDirectory;

    public MobileTemplateYamlLoader(string mobilesDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mobilesDirectory);

        _mobilesDirectory = Path.GetFullPath(mobilesDirectory);
    }

    public List<MobileTemplateDefinition> LoadAll()
    {
        if (!Directory.Exists(_mobilesDirectory))
        {
            _logger.Warning("Mobile template directory {Directory} not found; no templates loaded", _mobilesDirectory);

            return [];
        }

        var files = Directory.GetFiles(_mobilesDirectory, "*.yaml", SearchOption.AllDirectories)
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (files.Length == 0)
        {
            _logger.Warning("No mobile template files found in {Directory}", _mobilesDirectory);

            return [];
        }

        var templates = new List<MobileTemplateDefinition>();
        var sources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            MobileTemplateTable table;

            try
            {
                table = YamlUtils.DeserializeFromFile<MobileTemplateTable>(file);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to parse mobile template file '{file}'.", exception);
            }

            foreach (var template in table?.MobileTemplates ?? [])
            {
                if (string.IsNullOrWhiteSpace(template.Id))
                {
                    throw new InvalidOperationException($"Mobile template with empty id in '{file}'.");
                }

                if (sources.TryGetValue(template.Id, out var existingFile))
                {
                    throw new InvalidOperationException(
                        $"Duplicate mobile template id '{template.Id}' in '{file}' (already defined in '{existingFile}')."
                    );
                }

                template.Skills ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                template.Equipment ??= [];
                template.LootTables ??= [];
                template.Tags ??= [];
                template.Params ??= new Dictionary<string, ItemTemplateParamDefinition>(StringComparer.OrdinalIgnoreCase);

                sources[template.Id] = file;
                templates.Add(template);
            }
        }

        ResolveBaseMobiles(templates);

        _logger.Information("Loaded {Count} mobile templates from {FileCount} files", templates.Count, files.Length);

        return templates;
    }

    private static void ApplyInheritance(MobileTemplateDefinition parent, MobileTemplateDefinition child)
    {
        if (string.IsNullOrWhiteSpace(child.Name))
        {
            child.Name = parent.Name;
        }

        if (string.IsNullOrWhiteSpace(child.Title))
        {
            child.Title = parent.Title;
        }

        if (string.IsNullOrWhiteSpace(child.Brain))
        {
            child.Brain = parent.Brain;
        }

        if (string.IsNullOrWhiteSpace(child.FactionId))
        {
            child.FactionId = parent.FactionId;
        }

        if (string.IsNullOrWhiteSpace(child.BackpackTemplate))
        {
            child.BackpackTemplate = parent.BackpackTemplate;
        }

        child.Body = InheritInt(child.Body, parent.Body);
        child.RaceIndex = InheritInt(child.RaceIndex, parent.RaceIndex);
        child.SkinHue = InheritInt(child.SkinHue, parent.SkinHue);
        child.HairHue = InheritInt(child.HairHue, parent.HairHue);
        child.HairStyle = InheritInt(child.HairStyle, parent.HairStyle);
        child.FacialHairHue = InheritInt(child.FacialHairHue, parent.FacialHairHue);
        child.FacialHairStyle = InheritInt(child.FacialHairStyle, parent.FacialHairStyle);
        child.Karma = InheritInt(child.Karma, parent.Karma);
        child.Fame = InheritInt(child.Fame, parent.Fame);

        if (child.Gender == default)
        {
            child.Gender = parent.Gender;
        }

        if (child.Notoriety == NotorietyType.Innocent)
        {
            child.Notoriety = parent.Notoriety;
        }

        child.Stats ??= parent.Stats;
        child.Resources ??= parent.Resources;
        child.Resistances ??= parent.Resistances;

        if (child.Equipment.Count == 0 && parent.Equipment.Count > 0)
        {
            child.Equipment = [.. parent.Equipment];
        }

        if (child.LootTables.Count == 0 && parent.LootTables.Count > 0)
        {
            child.LootTables = [.. parent.LootTables];
        }

        if (child.Tags.Count == 0 && parent.Tags.Count > 0)
        {
            child.Tags = [.. parent.Tags];
        }

        child.Skills = MergeInts(parent.Skills, child.Skills);
        child.Params = MergeParams(parent.Params, child.Params);
    }

    private static int InheritInt(int childValue, int parentValue)
    {
        return childValue == 0 ? parentValue : childValue;
    }

    private static Dictionary<string, int> MergeInts(Dictionary<string, int> parent, Dictionary<string, int> child)
    {
        var merged = new Dictionary<string, int>(parent, StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in child)
        {
            merged[key] = value;
        }

        return merged;
    }

    private static Dictionary<string, ItemTemplateParamDefinition> MergeParams(
        Dictionary<string, ItemTemplateParamDefinition> parent,
        Dictionary<string, ItemTemplateParamDefinition> child
    )
    {
        var merged = new Dictionary<string, ItemTemplateParamDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in parent)
        {
            merged[key] = new ItemTemplateParamDefinition { Type = value.Type, Value = value.Value };
        }

        foreach (var (key, value) in child)
        {
            merged[key] = new ItemTemplateParamDefinition { Type = value.Type, Value = value.Value };
        }

        return merged;
    }

    private static void Resolve(
        MobileTemplateDefinition template,
        Dictionary<string, MobileTemplateDefinition> byId,
        Dictionary<string, ResolveState> states
    )
    {
        if (states.TryGetValue(template.Id, out var state))
        {
            if (state == ResolveState.Done)
            {
                return;
            }

            if (state == ResolveState.Visiting)
            {
                throw new InvalidOperationException($"Circular base_mobile reference detected at '{template.Id}'.");
            }
        }

        states[template.Id] = ResolveState.Visiting;

        if (!string.IsNullOrWhiteSpace(template.BaseMobile))
        {
            if (!byId.TryGetValue(template.BaseMobile, out var parent))
            {
                throw new InvalidOperationException(
                    $"Mobile template '{template.Id}' references unknown base_mobile '{template.BaseMobile}'."
                );
            }

            Resolve(parent, byId, states);
            ApplyInheritance(parent, template);
        }

        states[template.Id] = ResolveState.Done;
    }

    private static void ResolveBaseMobiles(List<MobileTemplateDefinition> templates)
    {
        var byId = templates.ToDictionary(
            static template => template.Id,
            static template => template,
            StringComparer.OrdinalIgnoreCase
        );

        var states = new Dictionary<string, ResolveState>(StringComparer.OrdinalIgnoreCase);

        foreach (var template in templates)
        {
            Resolve(template, byId, states);
        }
    }

    private enum ResolveState : byte
    {
        Unvisited = 0,
        Visiting = 1,
        Done = 2
    }
}
