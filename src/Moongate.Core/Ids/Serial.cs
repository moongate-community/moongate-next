using System.Globalization;
using System.Runtime.CompilerServices;
using Moongate.Core.Interfaces.Ids;

namespace Moongate.Core.Ids;

/// <summary>
///     Represents a UO entity serial identifier.
/// </summary>
public readonly struct Serial
    : IAutoIncrementKey<Serial>, IComparable<Serial>, IComparable<uint>, IEquatable<Serial>, ISpanFormattable,
        ISpanParsable<Serial>
{
    public const uint ItemOffset = 0x40000000;
    public const uint MaxItemSerial = 0x7EEEEEEE;
    public const uint MaxMobileSerial = ItemOffset - 1;
    public const int MobileStart = 0x00000001;

    public static readonly Serial ItemOffsetSerial = new(ItemOffset);
    public static readonly Serial MinusOne = new(0xFFFFFFFF);
    public static readonly Serial Zero = new(0);

    public Serial(uint serial)
    {
        Value = serial;
    }

    public uint Value { get; }

    public ulong Sequence => Value;

    public bool IsMobile
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value is > 0 and < ItemOffset;
    }

    public bool IsItem
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value is >= ItemOffset and <= MaxItemSerial;
    }

    public bool IsValid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Serial other)
    {
        return Value.CompareTo(other.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(uint other)
    {
        return Value.CompareTo(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Serial other)
    {
        return Value == other.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            Serial serial => this == serial,
            uint raw => Value == raw,
            _ => false
        };
    }

    public static Serial FromSequence(ulong value)
    {
        return new Serial((uint)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial operator +(Serial left, Serial right)
    {
        return (Serial)(left.Value + right.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial operator +(Serial left, uint right)
    {
        return (Serial)(left.Value + right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial operator --(Serial value)
    {
        return (Serial)(value.Value - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Serial left, Serial right)
    {
        return left.Value == right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Serial left, uint right)
    {
        return left.Value == right;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator uint(Serial value)
    {
        return value.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Serial(uint value)
    {
        return new Serial(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Serial left, Serial right)
    {
        return left.Value > right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Serial left, uint right)
    {
        return left.Value > right;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Serial left, Serial right)
    {
        return left.Value >= right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Serial left, uint right)
    {
        return left.Value >= right;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial operator ++(Serial value)
    {
        return (Serial)(value.Value + 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Serial left, Serial right)
    {
        return left.Value != right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Serial left, uint right)
    {
        return left.Value != right;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Serial left, Serial right)
    {
        return left.Value < right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Serial left, uint right)
    {
        return left.Value < right;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Serial left, Serial right)
    {
        return left.Value <= right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Serial left, uint right)
    {
        return left.Value <= right;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial operator -(Serial left, Serial right)
    {
        return (Serial)(left.Value - right.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial operator -(Serial left, uint right)
    {
        return (Serial)(left.Value - right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial Parse(string s)
    {
        return Parse(s, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial Parse(string s, IFormatProvider? provider)
    {
        return Parse(s.AsSpan(), provider);
    }

    public static Serial Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var serial))
        {
            return serial;
        }

        throw new FormatException("Input string was not in a correct serial format.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial RandomSerial()
    {
        return new Serial((uint)System.Random.Shared.Next(1, int.MaxValue));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ToInt32()
    {
        return (int)Value;
    }

    public override string ToString()
    {
        Span<char> destination = stackalloc char[10];
        TryFormat(destination, out var charsWritten, default, null);

        return destination[..charsWritten].ToString();
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString();
    }

    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    )
    {
        return format != ReadOnlySpan<char>.Empty
            ? Value.TryFormat(destination, out charsWritten, format, provider)
            : destination.TryWrite(provider, $"0x{Value:X8}", out charsWritten);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string? s, IFormatProvider? provider, out Serial result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Serial result)
    {
        _ = provider;

        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            s = s[2..];

            if (uint.TryParse(s, NumberStyles.HexNumber, null, out var hexValue))
            {
                result = new Serial(hexValue);

                return true;
            }
        }

        if (uint.TryParse(s, out var value))
        {
            result = new Serial(value);

            return true;
        }

        result = default;

        return false;
    }
}
