namespace Moongate.UO.Data.Types.Maps;

/// <summary>
/// Gameplay rule flags for a map facet (movement and beneficial/harmful action restrictions).
/// </summary>
[Flags]
public enum MapRulesType
{
    None = 0x0000,
    Internal = 0x0001,
    FreeMovement = 0x0002,
    BeneficialRestrictions = 0x0004,
    HarmfulRestrictions = 0x0008,
    TrammelRules = FreeMovement | BeneficialRestrictions | HarmfulRestrictions,
    FeluccaRules = None
}
