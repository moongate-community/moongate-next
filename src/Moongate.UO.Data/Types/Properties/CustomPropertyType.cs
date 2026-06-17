namespace Moongate.UO.Data.Types.Properties;

/// <summary>
///     Declares the supported value kind for a custom property.
/// </summary>
public enum CustomPropertyType : byte
{
    /// <summary>A 64-bit integer value.</summary>
    Integer = 0,

    /// <summary>A boolean value.</summary>
    Boolean = 1,

    /// <summary>A double-precision floating point value.</summary>
    Double = 2,

    /// <summary>A string value.</summary>
    String = 3
}
