using Moongate.UO.Data.Data.Loadouts;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Loadouts;
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

    public ValueTask ApplyAsync(
        MobileEntity mobile,
        StarterLoadout loadout,
        short shirtHue,
        short pantsHue,
        CancellationToken cancellationToken = default
    )
        => throw new NotImplementedException();

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
