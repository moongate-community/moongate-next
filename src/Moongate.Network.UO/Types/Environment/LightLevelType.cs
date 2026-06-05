namespace Moongate.Network.UO.Types.Environment;

/// <summary>
/// Defines UO client light levels.
/// </summary>
public enum LightLevelType : byte
{
    Day = 0x00,
    OsiNight = 0x09,
    Black = 0x1F
}
