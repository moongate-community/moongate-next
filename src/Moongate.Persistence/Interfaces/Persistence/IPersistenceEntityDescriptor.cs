namespace Moongate.Persistence.Interfaces.Persistence;

/// <summary>
/// Describes a registered persisted entity type.
/// </summary>
public interface IPersistenceEntityDescriptor
{
    /// <summary>Stable numeric identifier for the persisted entity kind.</summary>
    ushort TypeId { get; }

    /// <summary>Stable diagnostic name for the persisted entity kind.</summary>
    string TypeName { get; }

    /// <summary>Version of the persisted entity schema.</summary>
    int SchemaVersion { get; }

    /// <summary>CLR type of the entity.</summary>
    Type EntityType { get; }

    /// <summary>CLR type of the entity key.</summary>
    Type KeyType { get; }
}
