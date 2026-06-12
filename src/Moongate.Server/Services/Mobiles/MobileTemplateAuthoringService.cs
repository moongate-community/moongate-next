using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Data.Templates;
using Moongate.Server.Interfaces.Services.Templates;
using Moongate.Server.Services.Loot;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.Server.Services.Mobiles;

public sealed class MobileTemplateAuthoringService : IMobileTemplateAuthoringService
{
    private readonly MobileTemplateYamlDocumentStore _documents;
    private readonly IItemTemplateService _itemTemplates;
    private readonly LootTableRegistryStore _lootRegistryStore;
    private readonly string _mobilesDirectory;
    private readonly Lock _saveGate = new();
    private readonly IMobileTemplateService _templates;

    public MobileTemplateAuthoringService(
        DirectoriesConfig directories,
        IMobileTemplateService templates,
        IItemTemplateService itemTemplates,
        LootTableRegistryStore lootRegistryStore
    )
    {
        ArgumentNullException.ThrowIfNull(directories);
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(itemTemplates);
        ArgumentNullException.ThrowIfNull(lootRegistryStore);

        _mobilesDirectory = directories[DirectoryType.Templates_Mobiles];
        _documents = new MobileTemplateYamlDocumentStore(_mobilesDirectory);
        _templates = templates;
        _itemTemplates = itemTemplates;
        _lootRegistryStore = lootRegistryStore;
    }

    public ValueTask<MobileTemplateSaveResult> CreateAsync(
        MobileTemplateEditRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var def = ToDefinition(request);

        lock (_saveGate)
        {
            if (_templates.TryGet(def.Id, out _))
            {
                throw new InvalidOperationException($"Mobile template '{def.Id}' already exists.");
            }

            var sourceFile = Path.Combine(_mobilesDirectory, MobileTemplateYamlDocumentStore.ManagedFileName);

            return ValueTask.FromResult(Save(def, sourceFile, cancellationToken));
        }
    }

    public ValueTask<MobileTemplateSaveResult?> UpdateAsync(
        string id,
        MobileTemplateEditRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var def = ToDefinition(request);

        if (!string.Equals(id.Trim(), def.Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Route id must match the request id.");
        }

        lock (_saveGate)
        {
            if (!_templates.TryGet(id, out var existing))
            {
                return ValueTask.FromResult<MobileTemplateSaveResult?>(null);
            }

            if (!string.IsNullOrWhiteSpace(existing.BaseMobile))
            {
                throw new InvalidOperationException("base_mobile templates cannot be authored from the web editor yet.");
            }

            var sourceFile = _documents.ResolveSourceFile(def.Id);

            return ValueTask.FromResult<MobileTemplateSaveResult?>(Save(def, sourceFile, cancellationToken));
        }
    }

    private MobileTemplateSaveResult Save(
        MobileTemplateDefinition def,
        string sourceFile,
        CancellationToken cancellationToken
    )
    {
        Validate(def);
        cancellationToken.ThrowIfCancellationRequested();

        _documents.Upsert(sourceFile, def);

        var reloaded = new MobileTemplateYamlLoader(_mobilesDirectory).LoadAll();
        _templates.ReplaceAll(reloaded);

        var saved = reloaded.Single(
            t => string.Equals(t.Id, def.Id, StringComparison.OrdinalIgnoreCase)
        );

        var relative = Path.GetRelativePath(_mobilesDirectory, sourceFile)
                           .Replace(Path.DirectorySeparatorChar, '/');

        return new MobileTemplateSaveResult(MobileTemplateDetail.FromDefinition(saved), relative);
    }

    private static MobileTemplateDefinition ToDefinition(MobileTemplateEditRequest r)
    {
        var id = r.Id.Trim();

        var tags = (r.Tags ?? [])
                   .Select(static tag => tag.Trim())
                   .Where(static tag => tag.Length > 0)
                   .Distinct(StringComparer.OrdinalIgnoreCase)
                   .ToList();

        var skills = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in r.Skills ?? new(StringComparer.OrdinalIgnoreCase))
        {
            skills[key.Trim()] = value;
        }

        var parameters = new Dictionary<string, ItemTemplateParamDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, param) in r.Params ?? new(StringComparer.OrdinalIgnoreCase))
        {
            parameters[key.Trim()] = new ItemTemplateParamDefinition
            {
                Type = param.Type,
                Value = param.Value
            };
        }

        var equipment = (r.Equipment ?? [])
                        .Where(static item => !string.IsNullOrWhiteSpace(item))
                        .Select(static item => new MobileEquipmentEntry { Item = item.Trim() })
                        .ToList();

        var lootTables = (r.LootTables ?? [])
                         .Select(static id => id.Trim())
                         .Where(static id => id.Length > 0)
                         .ToList();

        return new MobileTemplateDefinition
        {
            Id = id,
            BaseMobile = string.IsNullOrWhiteSpace(r.BaseMobile) ? null : r.BaseMobile.Trim(),
            IsAbstract = r.IsAbstract,
            Name = r.Name,
            Title = r.Title,
            Body = r.Body,
            Gender = r.Gender,
            RaceIndex = r.RaceIndex,
            SkinHue = r.SkinHue,
            HairHue = r.HairHue,
            HairStyle = r.HairStyle,
            FacialHairHue = r.FacialHairHue,
            FacialHairStyle = r.FacialHairStyle,
            Brain = string.IsNullOrWhiteSpace(r.Brain) ? null : r.Brain.Trim(),
            Notoriety = r.Notoriety,
            Karma = r.Karma,
            Fame = r.Fame,
            FactionId = string.IsNullOrWhiteSpace(r.FactionId) ? null : r.FactionId.Trim(),
            Stats = r.Stats,
            Resources = r.Resources,
            Resistances = r.Resistances,
            Skills = skills,
            Equipment = equipment,
            BackpackTemplate = r.BackpackTemplate,
            LootTables = lootTables,
            Tags = tags,
            Params = parameters
        };
    }

    private void Validate(MobileTemplateDefinition def)
    {
        if (!string.IsNullOrWhiteSpace(def.BaseMobile))
        {
            throw new InvalidOperationException("base_mobile templates cannot be authored from the web editor yet.");
        }

        if (string.IsNullOrWhiteSpace(def.Id))
        {
            throw new InvalidOperationException("Mobile template id is required.");
        }

        if (def.Body <= 0)
        {
            throw new InvalidOperationException("body must be greater than 0.");
        }

        for (var i = 0; i < def.Equipment.Count; i++)
        {
            var item = def.Equipment[i].Item;

            if (!_itemTemplates.TryGet(item, out _))
            {
                throw new InvalidOperationException($"equipment item template '{item}' does not exist.");
            }
        }

        for (var i = 0; i < def.LootTables.Count; i++)
        {
            var lootId = def.LootTables[i];

            if (!_lootRegistryStore.Registry.TryGet(lootId, out _))
            {
                throw new InvalidOperationException($"loot table '{lootId}' does not exist.");
            }
        }

        foreach (var (key, _) in def.Params)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("params keys must not be blank.");
            }
        }
    }
}
