namespace Moongate.Core.Interfaces.Ids;

/// <summary>
/// Typed auto-increment key. Implement on custom ID structs to gain automatic serial allocation
/// in <c>IAutoDataAccess&lt;TEntity,TKey&gt;</c>.
/// </summary>
public interface IAutoIncrementKey<TSelf> : IAutoIncrementKey
    where TSelf : struct, IAutoIncrementKey<TSelf>
{
    /// <summary>Creates a key from the given sequence value.</summary>
    abstract static TSelf FromSequence(ulong value);
}
