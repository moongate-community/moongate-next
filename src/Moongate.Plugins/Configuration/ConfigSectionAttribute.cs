namespace Moongate.Plugins.Configuration;

/// <summary>
/// Marks a nested config object as a form section. Its <c>[ConfigField]</c> leaves become the
/// section's fields. The section id is the label lowercased (spaces → <c>_</c>).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigSectionAttribute : Attribute
{
    /// <summary>Human-readable section title shown in the admin UI.</summary>
    public string Label { get; }

    /// <summary>Optional explicit ordering; sections otherwise follow property declaration order.</summary>
    public int Order { get; set; }

    public ConfigSectionAttribute(string label)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        Label = label;
    }
}
