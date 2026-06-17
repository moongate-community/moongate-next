using Moongate.Core.Extensions.Strings;

namespace Moongate.Tests.Core.Extensions.Strings;

public class StringHelpersTests
{
    [Fact]
    public void Capitalize_EmptyInput_ReturnsEmpty()
    {
        Assert.Equal("", "".Capitalize());
    }

    [Fact]
    public void Capitalize_NullInput_ReturnsNull()
    {
        Assert.Null(((string?)null).Capitalize());
    }

    [Theory]
    [InlineData("hello world", "Hello World")]
    [InlineData("lord of the rings", "Lord Of the Rings")]
    [InlineData("the lord of the rings", "The Lord Of the Rings")]
    [InlineData("the lord", "The Lord")]
    [InlineData("a", "A")]
    public void Capitalize_VariousInputs_CapitalizesEachWordAndSkipsInternalThe(string input, string expected)
    {
        Assert.Equal(expected, input.Capitalize());
    }

    [Fact]
    public void DefaultIfNullOrEmpty_BlankInput_ReturnsDefault()
    {
        Assert.Equal("fallback", "".DefaultIfNullOrEmpty("fallback"));
        Assert.Equal("fallback", "   ".DefaultIfNullOrEmpty("fallback"));
    }

    [Fact]
    public void DefaultIfNullOrEmpty_NonBlankInput_ReturnsInput()
    {
        Assert.Equal("value", "value".DefaultIfNullOrEmpty("fallback"));
    }

    [Fact]
    public void IndentMultiline_PrependsIndentToEachLine()
    {
        var input = "one\ntwo\nthree";
        var result = input.IndentMultiline("  ");

        Assert.Equal("  one\n  two\n  three", result);
    }

    [Fact]
    public void Remove_BufferTooSmall_ThrowsArgumentException()
    {
        Span<char> tiny = stackalloc char[2];

        // The "abc" prefix would already exceed the destination buffer.
        // Wrapping in a static lambda to keep stackalloc valid would lose the Span ref;
        // instead, call into a local helper that throws as expected.
        var threw = false;

        try
        {
            "abcdef".AsSpan().Remove("X".AsSpan(), StringComparison.Ordinal, tiny, out _);
        }
        catch (ArgumentException)
        {
            threw = true;
        }

        Assert.True(threw);
    }

    [Theory]
    [InlineData("", "x", "")]
    [InlineData("abc", "", "abc")]
    [InlineData("aXbXcXdXe", "X", "abcde")]
    [InlineData("xyzabc123abc", "abc", "xyz123")]
    [InlineData("nothing here", "missing", "nothing here")]
    [InlineData("aaaa", "aa", "")]
    public void Remove_StringPatterns_RemovesAllOccurrences(string input, string pattern, string expected)
    {
        // Regression: previous implementation advanced by 1 instead of pattern.Length,
        // leaving stray characters in the output for multi-char patterns.
        var result = input.AsSpan().Remove(pattern.AsSpan(), StringComparison.Ordinal);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TrimMultiline_TrimsEachLine()
    {
        var input = "  one  \n two\nthree  ";
        var result = input.TrimMultiline();

        Assert.Equal("one\ntwo\nthree", result);
    }

    [Fact]
    public void Wrap_ShortText_FitsInSingleLine()
    {
        var lines = "abc def".Wrap(10, 5);

        Assert.NotNull(lines);
        Assert.Single(lines!);
        Assert.Equal("abc def", lines![0]);
    }
}
