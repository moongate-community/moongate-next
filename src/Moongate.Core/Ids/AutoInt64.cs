using Moongate.Core.Interfaces.Ids;

namespace Moongate.Core.Ids;

/// <summary>
/// Auto-increment wrapper around <see cref="long" /> for use as a persistence entity key.
/// </summary>
public readonly struct AutoInt64 : IAutoIncrementKey<AutoInt64>, IEquatable<AutoInt64>, IComparable<AutoInt64>
{
    public AutoInt64(long value)
    {
        Value = value;
    }

    public long Value { get; }

    public ulong Sequence => (ulong)Value;

    public int CompareTo(AutoInt64 other)
        => Value.CompareTo(other.Value);

    public bool Equals(AutoInt64 other)
        => Value == other.Value;

    public override bool Equals(object? obj)
        => obj is AutoInt64 other && Equals(other);

    public static AutoInt64 FromSequence(ulong value)
        => new((long)value);

    public override int GetHashCode()
        => Value.GetHashCode();

    public static bool operator ==(AutoInt64 left, AutoInt64 right)
        => left.Value == right.Value;

    public static explicit operator long(AutoInt64 value)
        => value.Value;

    public static explicit operator AutoInt64(long value)
        => new(value);

    public static bool operator !=(AutoInt64 left, AutoInt64 right)
        => left.Value != right.Value;

    public override string ToString()
        => Value.ToString();
}
