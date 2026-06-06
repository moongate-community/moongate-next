namespace Moongate.Abstractions.Data.Internal;

/// <summary>
/// Boot-time declaration of a config section: how to name it, default it, and bind it from YAML.
/// Accumulated in the container and consumed by the config loader at startup.
/// </summary>
public sealed class ConfigSectionRegistration
{
    private readonly Func<object> _defaultFactory;

    public ConfigSectionRegistration(string name, Type type, Func<object> defaultFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(defaultFactory);

        Name = name;
        Type = type;
        _defaultFactory = defaultFactory;
    }

    /// <summary>YAML mapping name for this section (e.g. <c>persistence</c>).</summary>
    public string Name { get; }

    /// <summary>CLR type of the config.</summary>
    public Type Type { get; }

    /// <summary>Creates a fresh default instance of the config.</summary>
    public object CreateDefault()
        => _defaultFactory();
}
