namespace Moongate.UO.Data.Types.Expansions;

#pragma warning disable CA1069 // Enums should not have duplicate values

/// <summary>Character-list screen capability flags per expansion.</summary>
[Flags]
public enum CharacterListFlags
{
    None = 0x00000000,
    Unknown1 = 0x00000001,
    OverwriteConfigButton = 0x00000002,
    OneCharacterSlot = 0x00000004,
    ContextMenus = 0x00000008,
    SlotLimit = 0x00000010,
    Aos = 0x00000020,
    SixthCharacterSlot = 0x00000040,
    Se = 0x00000080,
    Ml = 0x00000100,
    Kr = 0x00000200,
    Uo3DClientType = 0x00000400,
    Unknown3 = 0x00000800,
    SeventhCharacterSlot = 0x00001000,
    Unknown4 = 0x00002000,
    NewMovementSystem = 0x00004000,
    NewFeluccaAreas = 0x00008000,

    ExpansionNone = ContextMenus,
    ExpansionT2A = ContextMenus,
    ExpansionUor = ContextMenus,
    ExpansionUotd = ContextMenus,
    ExpansionLbr = ContextMenus,
    ExpansionAos = ContextMenus | Aos,
    ExpansionSe = ExpansionAos | Se,
    ExpansionMl = ExpansionSe | Ml,
    ExpansionSa = ExpansionMl,
    ExpansionHs = ExpansionSa,
    ExpansionTol = ExpansionHs,
    ExpansionEj = ExpansionTol
}
