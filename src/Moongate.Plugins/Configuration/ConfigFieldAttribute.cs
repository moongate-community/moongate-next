using Moongate.Plugins.Types;

namespace Moongate.Plugins.Configuration;

/// <summary>
///     Marks a config property as an editable form field. Type, default value, path and order are
///     derived unless overridden here.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigFieldAttribute : Attribute
{
    public ConfigFieldAttribute(string label)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        Label = label;
    }

    /// <summary>Human-readable field label shown in the admin UI.</summary>
    public string Label { get; }

    /// <summary>Field rendering type; <see cref="ConfigFieldType.Auto" /> infers it from the property type.</summary>
    public ConfigFieldType Type { get; set; } = ConfigFieldType.Auto;

    /// <summary>Whether the field is required.</summary>
    public bool Required { get; set; }

    /// <summary>Optional helper text.</summary>
    public string? Help { get; set; }

    /// <summary>Whether the value is a logical secret reference (rendered accordingly).</summary>
    public bool Secret { get; set; }

    /// <summary>Optional fixed set of allowed values (renders a dropdown).</summary>
    public string[]? Options { get; set; }

    /// <summary>Optional explicit ordering; fields otherwise follow property declaration order.</summary>
    public int Order { get; set; }
}
