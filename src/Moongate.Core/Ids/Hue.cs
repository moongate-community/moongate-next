using System.Globalization;
using System.Runtime.CompilerServices;

namespace Moongate.Core.Ids;

/// <summary>
/// Represents a UO color hue: a 16-bit palette index where 0 means "no hue"
/// (the object's default coloring).
/// </summary>
public readonly struct Hue
    : IComparable<Hue>, IEquatable<Hue>, ISpanFormattable, ISpanParsable<Hue>
{
    public const ushort MinValue = 0x0000;
    public const ushort MaxValue = 0xFFFF;

    public static readonly Hue None = new(0);

    public Hue(ushort value)
    {
        Value = value;
    }

    public ushort Value { get; }

    public bool IsNone
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Hue other)
        => Value.CompareTo(other.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Hue other)
        => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
        => obj switch
        {
            Hue hue    => Value == hue.Value,
            ushort raw => Value == raw,
            _          => false
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => Value.GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ToInt32()
        => Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Hue left, Hue right)
        => left.Value == right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Hue left, Hue right)
        => left.Value != right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Hue left, Hue right)
        => left.Value < right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Hue left, Hue right)
        => left.Value > right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Hue left, Hue right)
        => left.Value <= right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Hue left, Hue right)
        => left.Value >= right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Hue(ushort value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ushort(Hue value)
        => value.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Hue(int value)
        => new((ushort)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator int(Hue value)
        => value.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Hue Parse(string s)
        => Parse(s, null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Hue Parse(string s, IFormatProvider? provider)
        => Parse(s.AsSpan(), provider);

    public static Hue Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var hue))
        {
            return hue;
        }

        throw new FormatException("Input string was not in a correct hue format.");
    }

    public override string ToString()
    {
        Span<char> destination = stackalloc char[6];
        TryFormat(destination, out var charsWritten, default, null);

        return destination[..charsWritten].ToString();
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
        => ToString();

    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    )
        => format != ReadOnlySpan<char>.Empty
               ? Value.TryFormat(destination, out charsWritten, format, provider)
               : destination.TryWrite(provider, $"0x{Value:X4}", out charsWritten);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string? s, IFormatProvider? provider, out Hue result)
        => TryParse(s.AsSpan(), provider, out result);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Hue result)
    {
        _ = provider;

        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            s = s[2..];

            if (ushort.TryParse(s, NumberStyles.HexNumber, null, out var hexValue))
            {
                result = new(hexValue);

                return true;
            }
        }

        if (ushort.TryParse(s, out var value))
        {
            result = new(value);

            return true;
        }

        result = default;

        return false;
    }
}
