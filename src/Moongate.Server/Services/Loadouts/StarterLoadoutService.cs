using Moongate.UO.Data.Data.Loadouts;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Loadouts;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Loadouts;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Loadouts;

/// <summary>
/// Default starter loadout service: composes the additive base + race +
/// profession overlays into resolved items and applies them to a mobile.
/// </summary>
public sealed class StarterLoadoutService : IStarterLoadoutService
{
    private readonly ILogger _logger = Log.ForContext<StarterLoadoutService>();
    private readonly IItemTemplateService _templates;
    private readonly IItemFactoryService _itemFactory;
    private readonly IMobileService _mobiles;
    private readonly IItemService _items;

    private StarterLoadoutDefinition? _definition;

    public StarterLoadoutService(
        IItemTemplateService templates,
        IItemFactoryService itemFactory,
        IMobileService mobiles,
        IItemService items
    )
    {
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(itemFactory);
        ArgumentNullException.ThrowIfNull(mobiles);
        ArgumentNullException.ThrowIfNull(items);

        _templates = templates;
        _itemFactory = itemFactory;
        _mobiles = mobiles;
        _items = items;
    }

    public void SetDefinition(StarterLoadoutDefinition? definition)
        => _definition = definition;

    public StarterLoadout Compose(int raceIndex, string? professionName)
    {
        var loadout = new StarterLoadout();
        var definition = _definition;

        if (definition is null)
        {
            return loadout;
        }

        if (!string.IsNullOrWhiteSpace(definition.BackpackTemplate))
        {
            loadout.Backpack = Resolve(new LoadoutItemEntry { Template = definition.BackpackTemplate });
        }

        AppendSection(loadout, definition.Base);

        var raceKey = raceIndex switch
        {
            0 => "human",
            1 => "elf",
            2 => "gargoyle",
            _ => null
        };

        if (raceKey is not null && definition.Races.TryGetValue(raceKey, out var raceSection))
        {
            AppendSection(loadout, raceSection);
        }

        if (!string.IsNullOrWhiteSpace(professionName) &&
            definition.Professions.TryGetValue(professionName, out var professionSection))
        {
            AppendSection(loadout, professionSection);
        }

        return loadout;
    }

    public async ValueTask ApplyAsync(
        MobileEntity mobile,
        StarterLoadout loadout,
        short shirtHue,
        short pantsHue,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(mobile);
        ArgumentNullException.ThrowIfNull(loadout);

        ItemEntity? backpackEntity = null;

        if (loadout.Backpack is not null && loadout.Backpack.Layer is { } backpackLayer)
        {
            backpackEntity = await _itemFactory.CreateFromTemplateAsync(loadout.Backpack.Template.Id, cancellationToken);

            // Set before EquipAsync: the mobile upsert inside it persists the reference.
            mobile.BackpackId = backpackEntity.Id;

            await EquipOrWarnAsync(mobile, backpackEntity, backpackLayer, cancellationToken);
        }

        foreach (var entry in loadout.Equip)
        {
            if (entry.Layer is not { } layer)
            {
                continue;
            }

            var item = await _itemFactory.CreateFromTemplateAsync(entry.Template.Id, cancellationToken);
            item.Amount = entry.Amount;

            var packetHue = entry.PacketHue switch
            {
                PacketHueSource.Shirt => shirtHue,
                PacketHueSource.Pants => pantsHue,
                _ => (short)0
            };

            // EquipAsync upserts the item, persisting the hue/amount mutations.
            if (entry.PacketHue != PacketHueSource.None && packetHue != 0)
            {
                item.Hue = (ushort)packetHue;
            }

            await EquipOrWarnAsync(mobile, item, layer, cancellationToken);
        }

        foreach (var entry in loadout.BackpackItems)
        {
            if (backpackEntity is null)
            {
                _logger.Warning(
                    "Starter loadout has backpack items but no backpack; skipping '{Template}'",
                    entry.Template.Id
                );

                continue;
            }

            var item = await _itemFactory.CreateFromTemplateAsync(entry.Template.Id, cancellationToken);
            item.Amount = entry.Amount;

            var added = await _items.AddItemAsync(backpackEntity, item, default, cancellationToken);

            if (!added)
            {
                _logger.Warning(
                    "Could not add starter item '{Template}' to backpack {Backpack}",
                    entry.Template.Id,
                    backpackEntity.Id
                );
            }
        }
    }

    private async ValueTask EquipOrWarnAsync(
        MobileEntity mobile,
        ItemEntity item,
        ItemLayerType layer,
        CancellationToken cancellationToken
    )
    {
        var equipped = await _mobiles.EquipAsync(mobile, item, layer, cancellationToken);

        if (!equipped)
        {
            _logger.Warning("Could not equip starter item {Item} on layer {Layer}", item.Id, layer);
        }
    }

    private void AppendSection(StarterLoadout loadout, LoadoutSection section)
    {
        foreach (var entry in section.EquipItems)
        {
            loadout.Equip.Add(Resolve(entry));
        }

        foreach (var entry in section.BackpackItems)
        {
            loadout.BackpackItems.Add(Resolve(entry));
        }
    }

    private StarterLoadoutItem Resolve(LoadoutItemEntry entry)
    {
        if (!_templates.TryGet(entry.Template, out var template))
        {
            throw new InvalidOperationException(
                $"Starter loadout references unknown item template '{entry.Template}'."
            );
        }

        return new StarterLoadoutItem(template, entry.Amount ?? template.Amount, entry.PacketHue, template.Layer);
    }
}
