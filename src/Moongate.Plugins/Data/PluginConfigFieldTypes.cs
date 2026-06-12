namespace Moongate.Plugins.Data;

/// <summary>
/// Field-type tokens for <see cref="PluginConfigField.Type" />. Shared by every configurable plugin
/// when building its config form and matched verbatim by the admin UI renderer.
/// </summary>
public static class PluginConfigFieldTypes
{
    /// <summary>A checkbox / boolean toggle.</summary>
    public const string Boolean = "boolean";

    /// <summary>A numeric input.</summary>
    public const string Number = "number";

    /// <summary>A single-line text input.</summary>
    public const string Text = "text";

    /// <summary>A multi-line text area.</summary>
    public const string TextArea = "textarea";
}
