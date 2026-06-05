using Moongate.Core.Extensions.Strings;

namespace Moongate.Tests.Core.Extensions.Strings;

public class OrdinalStringHelpersTests
{
    [Theory, InlineData("Hello World", "World", true), InlineData("Hello World", "WORLD", false)]
    public void ContainsOrdinal_String_CaseSensitive(string a, string b, bool expected)
        => Assert.Equal(expected, a.ContainsOrdinal(b));

    [Theory, InlineData("Hello", "lo", true), InlineData("Hello", "LO", false)]
    public void EndsWithOrdinal_CaseSensitive(string a, string b, bool expected)
        => Assert.Equal(expected, a.EndsWithOrdinal(b));

    [Fact]
    public void EndsWithOrdinal_SpanChar_MatchesLastChar()
    {
        Assert.True("abc".AsSpan().EndsWithOrdinal('c'));
        Assert.False("abc".AsSpan().EndsWithOrdinal('z'));
    }

    [Fact]
    public void EqualsOrdinal_BothNull_ReturnsTrue()
        => Assert.True(((string?)null).EqualsOrdinal(null));

    [Theory, InlineData("Hello", "Hello", true), InlineData("Hello", "hello", false), InlineData("Hello", "World", false)]
    public void EqualsOrdinal_CaseSensitive(string a, string b, bool expected)
        => Assert.Equal(expected, a.EqualsOrdinal(b));

    [Fact]
    public void IndexOfOrdinal_FindsExact()
    {
        Assert.Equal(6, "Hello World".IndexOfOrdinal("World"));
        Assert.Equal(-1, "Hello World".IndexOfOrdinal("world"));
    }

    [Fact]
    public void RemoveOrdinal_String_RemovesAll()
        => Assert.Equal("Hello", "Hello World".RemoveOrdinal(" World"));

    [Fact]
    public void ReplaceOrdinal_ReplacesAll()
        => Assert.Equal("HiHi", "abab".ReplaceOrdinal("ab", "Hi"));

    [Theory, InlineData("Hello", "He", true), InlineData("Hello", "he", false)]
    public void StartsWithOrdinal_CaseSensitive(string a, string b, bool expected)
        => Assert.Equal(expected, a.StartsWithOrdinal(b));

    [Fact]
    public void StartsWithOrdinal_SpanChar_MatchesFirstChar()
    {
        Assert.True("abc".AsSpan().StartsWithOrdinal('a'));
        Assert.False("abc".AsSpan().StartsWithOrdinal('z'));
    }
}
