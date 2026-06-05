namespace Moongate.Abstractions.Data.Internal;

/// <summary>
/// One loaded config section: the CLR type and the bound instance to register in DI.
/// </summary>
public sealed class ConfigLoadResult
{
    public ConfigLoadResult(Type type, object instance)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(instance);

        Type = type;
        Instance = instance;
    }

    public Type Type { get; }
    public object Instance { get; }
}
