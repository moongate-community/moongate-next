using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Loadouts;

namespace Moongate.UO.Data.Data.Loadouts;

/// <summary>
/// One resolved starter loadout entry: the item template plus effective amount,
/// packet hue source and equip layer (taken from the template).
/// </summary>
public sealed class StarterLoadoutItem
{
    public ItemTemplateDefinition Template { get; }

    public int Amount { get; }

    public PacketHueSource PacketHue { get; }

    public ItemLayerType? Layer { get; }

    public StarterLoadoutItem(
        ItemTemplateDefinition template,
        int amount,
        PacketHueSource packetHue,
        ItemLayerType? layer
    )
    {
        ArgumentNullException.ThrowIfNull(template);

        Template = template;
        Amount = amount;
        PacketHue = packetHue;
        Layer = layer;
    }
}
