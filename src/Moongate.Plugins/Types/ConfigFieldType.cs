namespace Moongate.Plugins.Types;

/// <summary>
/// Form-field rendering hint for a config property. <see cref="Auto" /> infers the concrete type
/// from the property's CLR type (bool → boolean, integer → number, string → text).
/// </summary>
public enum ConfigFieldType
{
    /// <summary>Infer the field type from the property's CLR type.</summary>
    Auto,

    /// <summary>A checkbox / boolean toggle.</summary>
    Boolean,

    /// <summary>A numeric input.</summary>
    Number,

    /// <summary>A single-line text input.</summary>
    Text,

    /// <summary>A multi-line text area.</summary>
    TextArea
}
