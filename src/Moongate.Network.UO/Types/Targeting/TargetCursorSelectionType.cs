namespace Moongate.Network.UO.Types.Targeting;

/// <summary>
/// Indicates whether the target cursor expects an object or a world position.
/// </summary>
public enum TargetCursorSelectionType : byte
{
    SelectObject = 0,
    SelectLocation = 1
}
