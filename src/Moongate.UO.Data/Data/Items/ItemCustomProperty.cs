using Moongate.UO.Data.Types.Items;

namespace Moongate.UO.Data.Data.Items;

/// <summary>
/// A typed custom value attached to an item: the <see cref="Type" /> selects
/// which of the value fields is meaningful.
/// </summary>
public sealed class ItemCustomProperty
{
    /// <summary>Kind of value held by this property.</summary>
    public ItemCustomPropertyType Type { get; set; }

    /// <summary>Value used when <see cref="Type" /> is <see cref="ItemCustomPropertyType.Integer" />.</summary>
    public long IntegerValue { get; set; }

    /// <summary>Value used when <see cref="Type" /> is <see cref="ItemCustomPropertyType.Boolean" />.</summary>
    public bool BooleanValue { get; set; }

    /// <summary>Value used when <see cref="Type" /> is <see cref="ItemCustomPropertyType.Double" />.</summary>
    public double DoubleValue { get; set; }

    /// <summary>Value used when <see cref="Type" /> is <see cref="ItemCustomPropertyType.String" />.</summary>
    public string? StringValue { get; set; }
}
