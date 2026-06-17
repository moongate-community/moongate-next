namespace Moongate.UO.Data.Types.Localization;

/// <summary>
///     Origin marker for a cliloc string entry.
/// </summary>
[Flags]
public enum CliLocFlagType
{
    Original = 0x0,
    Custom = 0x1,
    Modified = 0x2
}
