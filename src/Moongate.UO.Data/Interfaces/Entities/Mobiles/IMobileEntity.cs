using Moongate.Core.Ids;
using Moongate.UO.Data.Data;
using Moongate.UO.Data.Data.Mobiles;
using Moongate.UO.Data.Interfaces.Entities.Base;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Mobiles;
using Moongate.UO.Data.Types.Skills;

namespace Moongate.UO.Data.Interfaces.Entities.Mobiles;

/// <summary>
///     Data contract for mobile entities (players and NPCs): identity, appearance,
///     vitals/stats/skills and equipment references. Derived facts (effective stats,
///     body-from-race, skill checks) and behavior live in the mobile service.
/// </summary>
public interface IMobileEntity : IWorldEntity
{
    /// <summary>Serial of the owning account.</summary>
    Serial AccountId { get; set; }

    /// <summary>Optional title shown after the name.</summary>
    string? Title { get; set; }

    /// <summary>Body graphic id.</summary>
    int BodyId { get; set; }

    /// <summary>Gender of the mobile.</summary>
    GenderType Gender { get; set; }

    /// <summary>Race index; the rich race object is resolved by the race registry.</summary>
    int RaceIndex { get; set; }

    /// <summary>Body/skin hue.</summary>
    Hue SkinHue { get; set; }

    /// <summary>Hair graphic id.</summary>
    int HairStyle { get; set; }

    /// <summary>Hair hue.</summary>
    Hue HairHue { get; set; }

    /// <summary>Facial hair graphic id.</summary>
    int FacialHairStyle { get; set; }

    /// <summary>Facial hair hue.</summary>
    Hue FacialHairHue { get; set; }

    /// <summary>True when the mobile is a player character.</summary>
    bool IsPlayer { get; set; }

    /// <summary>True when the mobile is alive.</summary>
    bool IsAlive { get; set; }

    /// <summary>Base attributes (strength/dexterity/intelligence).</summary>
    MobileStats BaseStats { get; set; }

    /// <summary>Damage resistances.</summary>
    MobileResistances Resistances { get; set; }

    /// <summary>Vital pools (hits/mana/stamina).</summary>
    MobileResources Resources { get; set; }

    /// <summary>Total stat cap.</summary>
    int StatCap { get; set; }

    /// <summary>Raise/lower/lock state for strength.</summary>
    UOSkillLock StrengthLock { get; set; }

    /// <summary>Raise/lower/lock state for dexterity.</summary>
    UOSkillLock DexterityLock { get; set; }

    /// <summary>Raise/lower/lock state for intelligence.</summary>
    UOSkillLock IntelligenceLock { get; set; }

    /// <summary>Skills keyed by skill name.</summary>
    Dictionary<UOSkillName, SkillEntry> Skills { get; set; }

    /// <summary>Equipped item serials keyed by paperdoll layer.</summary>
    Dictionary<ItemLayerType, Serial> EquippedItemIds { get; set; }

    /// <summary>Serial of the mobile's backpack container.</summary>
    Serial BackpackId { get; set; }

    /// <summary>Typed custom properties keyed by name.</summary>
    Dictionary<string, CustomProperty> CustomProperties { get; set; }
}
