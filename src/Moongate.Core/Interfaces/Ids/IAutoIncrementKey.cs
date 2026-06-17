namespace Moongate.Core.Interfaces.Ids;

/// <summary>
///     Non-generic marker for runtime checks in type-erased persistence code.
/// </summary>
public interface IAutoIncrementKey
{
    /// <summary>The current key value as a monotonic 64-bit sequence.</summary>
    ulong Sequence { get; }
}
