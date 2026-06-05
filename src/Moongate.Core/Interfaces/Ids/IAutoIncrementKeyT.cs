namespace Moongate.Core.Interfaces.Ids;

/// <summary>
/// Typed auto-increment key. Implement on custom ID structs to gain automatic serial allocation
/// in <see cref="Moongate.Persistence.Interfaces.Persistence.IAutoDataAccess{TEntity,TKey}" />.
/// </summary>
public interface IAutoIncrementKey<TSelf> : IAutoIncrementKey
    where TSelf : struct, IAutoIncrementKey<TSelf>
{
    /// <summary>Creates a key from the given sequence value.</summary>
    abstract static TSelf FromSequence(ulong value);
}
