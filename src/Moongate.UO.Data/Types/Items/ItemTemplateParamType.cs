namespace Moongate.UO.Data.Types.Items;

/// <summary>
///     Declares the supported value kind for an item template param.
/// </summary>
public enum ItemTemplateParamType : byte
{
    /// <summary>A raw string value.</summary>
    String = 0,

    /// <summary>A 64-bit integer value (decimal or 0x-prefixed hex).</summary>
    Integer = 1,

    /// <summary>A hue palette index (decimal or 0x-prefixed hex).</summary>
    Hue = 2,

    /// <summary>A serial value (decimal or 0x-prefixed hex).</summary>
    Serial = 3
}
