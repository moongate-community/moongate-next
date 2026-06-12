using Moongate.Core.Ids;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Mobiles;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Properties;
using Moongate.UO.Data.Types.Skills;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Mobiles;

/// <summary>
/// Default mobile factory: maps a resolved mobile template onto a new persisted
/// non-player <see cref="MobileEntity" />, creating and equipping its backpack
/// and equipment and storing loot-table references and params as custom properties.
/// </summary>
public sealed class MobileFactoryService : IMobileFactoryService
{
    private readonly ILogger _logger = Log.ForContext<MobileFactoryService>();
    private readonly IMobileTemplateService _templates;
    private readonly IItemTemplateService _items;
    private readonly Lazy<IMobileService> _mobiles;
    private readonly Lazy<IItemFactoryService> _itemFactory;

    public MobileFactoryService(
        IMobileTemplateService templates,
        IItemTemplateService items,
        Lazy<IMobileService> mobiles,
        Lazy<IItemFactoryService> itemFactory
    )
    {
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(mobiles);
        ArgumentNullException.ThrowIfNull(itemFactory);

        _templates = templates;
        _items = items;
        _mobiles = mobiles;
        _itemFactory = itemFactory;
    }

    public async ValueTask<MobileEntity> CreateFromTemplateAsync(
        string templateId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);

        if (!_templates.TryGet(templateId, out var template))
        {
            throw new InvalidOperationException($"Mobile template '{templateId}' not found.");
        }

        if (template.IsAbstract)
        {
            throw new InvalidOperationException($"Mobile template '{templateId}' is abstract and cannot be instantiated.");
        }

        var mobile = BuildMobile(template);

        await _mobiles.Value.CreateAsync(mobile, cancellationToken);

        await EquipBackpackAsync(mobile, template, cancellationToken);
        await EquipItemsAsync(mobile, template, cancellationToken);

        return mobile;
    }

    private MobileEntity BuildMobile(MobileTemplateDefinition template)
    {
        var stats = template.Stats ?? new MobileStatsTemplate();
        var resources = template.Resources ?? new MobileResourcesTemplate();
        var resistances = template.Resistances ?? new MobileResistancesTemplate();

        var mobile = new MobileEntity
        {
            Name = template.Name,
            Title = template.Title,
            BodyId = template.Body,
            Gender = template.Gender,
            RaceIndex = template.RaceIndex,
            SkinHue = (Hue)template.SkinHue,
            HairHue = (Hue)template.HairHue,
            HairStyle = template.HairStyle,
            FacialHairHue = (Hue)template.FacialHairHue,
            FacialHairStyle = template.FacialHairStyle,
            IsPlayer = false,
            IsAlive = true,
            BrainId = template.Brain,
            Notoriety = template.Notoriety,
            Karma = template.Karma,
            Fame = template.Fame,
            FactionId = template.FactionId,
            BaseStats = new()
            {
                Strength = stats.Strength,
                Dexterity = stats.Dexterity,
                Intelligence = stats.Intelligence
            },
            Resources = new()
            {
                Hits = resources.Hits,
                MaxHits = resources.Hits,
                Mana = resources.Mana,
                MaxMana = resources.Mana,
                Stamina = resources.Stamina,
                MaxStamina = resources.Stamina
            },
            Resistances = new()
            {
                Physical = resistances.Physical,
                Fire = resistances.Fire,
                Cold = resistances.Cold,
                Poison = resistances.Poison,
                Energy = resistances.Energy
            }
        };

        foreach (var (skillName, value) in template.Skills)
        {
            var skill = Enum.Parse<UOSkillName>(skillName, true);
            mobile.Skills[skill] = new() { Base = value, Value = value };
        }

        if (template.LootTables.Count > 0)
        {
            mobile.CustomProperties[MobileTemplateDefinitionKeys.LootTables] = new()
            {
                Type = CustomPropertyType.String,
                StringValue = string.Join(',', template.LootTables)
            };
        }

        foreach (var (key, param) in template.Params)
        {
            mobile.CustomProperties[key] = TemplateParamConverter.ToCustomProperty(param);
        }

        return mobile;
    }

    private async ValueTask EquipBackpackAsync(
        MobileEntity mobile,
        MobileTemplateDefinition template,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(template.BackpackTemplate))
        {
            return;
        }

        if (ResolveLayer(template.BackpackTemplate) is not { } layer)
        {
            _logger.Warning("Backpack template '{Template}' has no layer; skipping", template.BackpackTemplate);

            return;
        }

        var backpack = await _itemFactory.Value.CreateFromTemplateAsync(template.BackpackTemplate, cancellationToken);
        mobile.BackpackId = backpack.Id;

        await EquipOrWarnAsync(mobile, backpack, layer, cancellationToken);
    }

    private async ValueTask EquipItemsAsync(
        MobileEntity mobile,
        MobileTemplateDefinition template,
        CancellationToken cancellationToken
    )
    {
        foreach (var entry in template.Equipment)
        {
            if (ResolveLayer(entry.Item) is not { } layer)
            {
                _logger.Warning("Equipment '{Item}' has no layer; skipping", entry.Item);

                continue;
            }

            var item = await _itemFactory.Value.CreateFromTemplateAsync(entry.Item, cancellationToken);

            await EquipOrWarnAsync(mobile, item, layer, cancellationToken);
        }
    }

    private async ValueTask EquipOrWarnAsync(
        MobileEntity mobile,
        ItemEntity item,
        ItemLayerType layer,
        CancellationToken cancellationToken
    )
    {
        var equipped = await _mobiles.Value.EquipAsync(mobile, item, layer, cancellationToken);

        if (!equipped)
        {
            _logger.Warning("Could not equip '{Item}' on layer {Layer}", item.Id, layer);
        }
    }

    private ItemLayerType? ResolveLayer(string itemTemplateId)
        => _items.TryGet(itemTemplateId, out var item) ? item.Layer : null;
}
