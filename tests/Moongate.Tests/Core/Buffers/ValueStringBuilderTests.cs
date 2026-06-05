using Moongate.Core.Buffers;

namespace Moongate.Tests.Core.Buffers;

public class ValueStringBuilderTests
{
    [Fact]
    public void Append_CharRepeat_FillsWithChar()
    {
        using var sb = ValueStringBuilder.Create();
        sb.Append('=', 5);

        Assert.Equal("=====", sb.ToString());
    }

    [Fact]
    public void Append_SpanFitsInInitialBuffer_DoesNotGrow()
    {
        Span<char> buffer = stackalloc char[16];
        using var sb = new ValueStringBuilder(buffer);
        sb.Append("hello");

        Assert.Equal(16, sb.Capacity);
        Assert.Equal("hello", sb.ToString());
    }

    [Fact]
    public void Append_SpanLargerThanInitialCapacity_Grows()
    {
        Span<char> buffer = stackalloc char[8];
        using var sb = new ValueStringBuilder(buffer);
        sb.Append(new('x', 100));

        Assert.Equal(100, sb.Length);
        Assert.Equal(new('x', 100), sb.ToString());
    }

    [Fact]
    public void Append_String_AppendsToBuffer()
    {
        using var sb = ValueStringBuilder.Create();
        sb.Append("Hello");
        sb.Append(", ");
        sb.Append("world!");

        Assert.Equal("Hello, world!", sb.ToString());
        Assert.Equal(13, sb.Length);
    }

    [Fact]
    public void AppendInterpolated_FormatsValues()
    {
        using var sb = ValueStringBuilder.Create();
        var n = 42;
        sb.Append($"value={n}");

        Assert.Equal("value=42", sb.ToString());
    }

    [Fact]
    public void AsSpan_ReturnsWrittenOnly()
    {
        using var sb = ValueStringBuilder.Create();
        sb.Append("xyz");

        Assert.Equal("xyz", sb.AsSpan().ToString());
        Assert.Equal(3, sb.AsSpan().Length);
    }

    [Fact]
    public void Insert_AtMiddle_ShiftsTail()
    {
        using var sb = ValueStringBuilder.Create();
        sb.Append("abcdef");
        sb.Insert(3, "ZZ");

        Assert.Equal("abcZZdef", sb.ToString());
    }

    [Fact]
    public void Remove_FromMiddle_CompactsBuffer()
    {
        using var sb = ValueStringBuilder.Create();
        sb.Append("abcdef");
        sb.Remove(2, 2);

        Assert.Equal("abef", sb.ToString());
    }

    [Fact]
    public void Remove_FromStart_TruncatesPrefix()
    {
        using var sb = ValueStringBuilder.Create();
        sb.Append("abcdef");
        sb.Remove(0, 3);

        Assert.Equal("def", sb.ToString());
    }

    [Fact]
    public void Remove_FromTail_TruncatesSuffix()
    {
        using var sb = ValueStringBuilder.Create();
        sb.Append("abcdef");
        sb.Remove(3, 3);

        Assert.Equal("abc", sb.ToString());
    }

    [Fact]
    public void Replace_CharFullRange_ReplacesAll()
    {
        using var sb = ValueStringBuilder.Create();
        sb.Append("aaaa");

        sb.Replace('a', 'b', 0, sb.Length);

        Assert.Equal("bbbb", sb.ToString());
    }

    [Fact]
    public void Replace_CharWithinSubrange_OnlyReplacesInRange()
    {
        // Regression: Replace used to scan the entire underlying buffer (capacity),
        // ignoring startIndex/count and reading past Length.
        using var sb = ValueStringBuilder.Create();
        sb.Append("abcabcabc");

        sb.Replace('a', 'X', 3, 3);

        Assert.Equal("abcXbcabc", sb.ToString());
    }

    [Fact]
    public void Replace_StartIndexBeyondLength_Throws()
    {
        using var sb = ValueStringBuilder.Create();
        sb.Append("abc");

        // ValueStringBuilder is a ref struct, so Assert.Throws's lambda cannot capture it.
        var threw = false;

        try
        {
            sb.Replace('a', 'b', 99, 0);
        }
        catch (ArgumentOutOfRangeException)
        {
            threw = true;
        }

        Assert.True(threw);
    }

    [Fact]
    public void ReplaceAny_WithinSubrange_OnlyReplacesInRange()
    {
        // Regression: same bug as Replace.
        using var sb = ValueStringBuilder.Create();
        sb.Append("aXbXcXdXe");

        sb.ReplaceAny("abc", "ABC", 0, 5);

        Assert.Equal("AXBXCXdXe", sb.ToString());
    }

    [Fact]
    public void Reset_ZeroesLengthWithoutLosingCapacity()
    {
        using var sb = ValueStringBuilder.Create();
        sb.Append("hello");
        var capacityBefore = sb.Capacity;

        sb.Reset();

        Assert.Equal(0, sb.Length);
        Assert.Equal(capacityBefore, sb.Capacity);
    }
}
