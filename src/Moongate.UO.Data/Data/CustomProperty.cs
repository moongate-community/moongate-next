using Moongate.UO.Data.Types.Properties;

namespace Moongate.UO.Data.Data;

/// <summary>
///     A typed custom value attached to an entity (item or mobile): the
///     <see cref="Type" /> selects which value field is meaningful.
/// </summary>
public sealed class CustomProperty
{
    /// <summary>Kind of value held by this property.</summary>
    public CustomPropertyType Type { get; set; }

    /// <summary>Value used when <see cref="Type" /> is <see cref="CustomPropertyType.Integer" />.</summary>
    public long IntegerValue { get; set; }

    /// <summary>Value used when <see cref="Type" /> is <see cref="CustomPropertyType.Boolean" />.</summary>
    public bool BooleanValue { get; set; }

    /// <summary>Value used when <see cref="Type" /> is <see cref="CustomPropertyType.Double" />.</summary>
    public double DoubleValue { get; set; }

    /// <summary>Value used when <see cref="Type" /> is <see cref="CustomPropertyType.String" />.</summary>
    public string? StringValue { get; set; }
}
