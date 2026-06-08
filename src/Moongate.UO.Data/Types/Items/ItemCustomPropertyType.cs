namespace Moongate.UO.Data.Types.Items;

/// <summary>
/// Declares the supported value kind for an item custom property.
/// </summary>
public enum ItemCustomPropertyType : byte
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
