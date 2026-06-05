using Moongate.Core.Interfaces.Ids;

namespace Moongate.Core.Ids;

/// <summary>
/// Auto-increment wrapper around <see cref="int" /> for use as a persistence entity key.
/// </summary>
public readonly struct AutoInt32 : IAutoIncrementKey<AutoInt32>, IEquatable<AutoInt32>, IComparable<AutoInt32>
{
    public AutoInt32(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public ulong Sequence => (ulong)Value;

    public int CompareTo(AutoInt32 other)
        => Value.CompareTo(other.Value);

    public bool Equals(AutoInt32 other)
        => Value == other.Value;

    public override bool Equals(object? obj)
        => obj is AutoInt32 other && Equals(other);

    public static AutoInt32 FromSequence(ulong value)
        => new((int)value);

    public override int GetHashCode()
        => Value.GetHashCode();

    public static bool operator ==(AutoInt32 left, AutoInt32 right)
        => left.Value == right.Value;

    public static explicit operator int(AutoInt32 value)
        => value.Value;

    public static explicit operator AutoInt32(int value)
        => new(value);

    public static bool operator !=(AutoInt32 left, AutoInt32 right)
        => left.Value != right.Value;

    public override string ToString()
        => Value.ToString();
}
