using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.UO.Data.Data;
using Moongate.UO.Data.Data.Mobiles;
using Moongate.UO.Data.Interfaces.Entities.Mobiles;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Mobiles;
using Moongate.UO.Data.Types.Skills;

namespace Moongate.UO.Data.Entities.Mobiles;

/// <summary>
/// Concrete persisted mobile entity. Holds mobile state only; derived facts and
/// behavior are provided by the mobile service. Serialized via contractless MessagePack.
/// </summary>
public sealed class MobileEntity : IMobileEntity
{
    public Serial Id { get; set; }

    public string? Name { get; set; }

    public DirectionType Direction { get; set; }

    public Point3D Location { get; set; }

    public Hue Hue { get; set; }

    public int MapId { get; set; }

    public Serial AccountId { get; set; }

    public string? Title { get; set; }

    public int BodyId { get; set; }

    public GenderType Gender { get; set; }

    public int RaceIndex { get; set; }

    public Hue SkinHue { get; set; }

    public int HairStyle { get; set; }

    public Hue HairHue { get; set; }

    public int FacialHairStyle { get; set; }

    public Hue FacialHairHue { get; set; }

    public bool IsPlayer { get; set; }

    public bool IsAlive { get; set; } = true;

    public MobileStats BaseStats { get; set; } = new();

    public MobileResistances Resistances { get; set; } = new();

    public MobileResources Resources { get; set; } = new();

    public int StatCap { get; set; }

    public UOSkillLock StrengthLock { get; set; } = UOSkillLock.Up;

    public UOSkillLock DexterityLock { get; set; } = UOSkillLock.Up;

    public UOSkillLock IntelligenceLock { get; set; } = UOSkillLock.Up;

    public Dictionary<UOSkillName, SkillEntry> Skills { get; set; } = [];

    public Dictionary<ItemLayerType, Serial> EquippedItemIds { get; set; } = [];

    public Serial BackpackId { get; set; }

    public Dictionary<string, CustomProperty> CustomProperties { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
