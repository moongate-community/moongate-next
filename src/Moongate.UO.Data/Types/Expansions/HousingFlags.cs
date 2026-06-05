namespace Moongate.UO.Data.Types.Expansions;

/// <summary>Custom-housing capability flags per expansion.</summary>
[Flags]
public enum HousingFlags
{
    None = 0x0,
    Aos = 0x10,
    Se = 0x40,
    Ml = 0x80,
    Crystal = 0x200,
    Sa = 0x10000,
    Hs = 0x20000,
    Gothic = 0x40000,
    Rustic = 0x80000,
    Jungle = 0x100000,
    Shadowguard = 0x200000,
    Tol = 0x400000,
    Ej = 0x800000,

    HousingAos = Aos,
    HousingSe = HousingAos | Se,
    HousingMl = HousingSe | Ml | Crystal,
    HousingSa = HousingMl | Sa | Gothic | Rustic,
    HousingHs = HousingSa | Hs,
    HousingTol = HousingHs | Tol | Jungle | Shadowguard,
    HousingEj = HousingTol | Ej
}
