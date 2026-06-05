namespace Moongate.Network.UO.Types.Environment;

/// <summary>
/// Defines UO weather modes.
/// </summary>
public enum WeatherType : byte
{
    Rain = 0x00,
    Storm = 0x01,
    Snow = 0x02,
    StormBrewing = 0x03,
    NoEffect = 0xFE,
    None = 0xFF
}
