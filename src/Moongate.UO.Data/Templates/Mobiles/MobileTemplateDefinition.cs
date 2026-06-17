using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Mobiles;

namespace Moongate.UO.Data.Templates.Mobiles;

/// <summary>
///     Declarative NPC mobile template loaded from YAML. The <see cref="BaseMobile" />
///     chain is resolved at boot before the template enters the registry.
/// </summary>
public sealed class MobileTemplateDefinition
{
    public string Id { get; set; } = "";

    public string? BaseMobile { get; set; }

    public bool IsAbstract { get; set; }

    public string? Name { get; set; }

    public string? Title { get; set; }

    public int Body { get; set; }

    public GenderType Gender { get; set; }

    public int RaceIndex { get; set; }

    public int SkinHue { get; set; }

    public int HairHue { get; set; }

    public int HairStyle { get; set; }

    public int FacialHairHue { get; set; }

    public int FacialHairStyle { get; set; }

    public string? Brain { get; set; }

    public NotorietyType Notoriety { get; set; } = NotorietyType.Innocent;

    public int Karma { get; set; }

    public int Fame { get; set; }

    public string? FactionId { get; set; }

    public MobileStatsTemplate? Stats { get; set; }

    public MobileResourcesTemplate? Resources { get; set; }

    public MobileResistancesTemplate? Resistances { get; set; }

    public Dictionary<string, int> Skills { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public List<MobileEquipmentEntry> Equipment { get; set; } = [];

    public string? BackpackTemplate { get; set; }

    public List<string> LootTables { get; set; } = [];

    public List<string> Tags { get; set; } = [];

    public Dictionary<string, ItemTemplateParamDefinition> Params { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
