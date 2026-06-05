using Moongate.Core.Extensions.Strings;

namespace Moongate.Tests.Core.Extensions.Strings;

public class InsensitiveStringHelpersTests
{
    [Fact]
    public void InsensitiveContains_NullReceiver_ReturnsFalse()
        => Assert.False(((string?)null).InsensitiveContains("hi"));

    [Theory, InlineData("Hello World", "WORLD", true), InlineData("Hello World", "xyz", false)]
    public void InsensitiveContains_String_ChecksCaseInsensitive(string a, string b, bool expected)
        => Assert.Equal(expected, a.InsensitiveContains(b));

    [Theory, InlineData("Hello", "LO", true), InlineData("Hello", "HE", false)]
    public void InsensitiveEndsWith_ChecksSuffix(string a, string b, bool expected)
        => Assert.Equal(expected, a.InsensitiveEndsWith(b));

    [Fact]
    public void InsensitiveEquals_BothNull_ReturnsTrue()
        => Assert.True(((string?)null).InsensitiveEquals(null));

    [Theory, InlineData("Hello", "hello", true), InlineData("Hello", "HELLO", true), InlineData("Hello", "World", false)]
    public void InsensitiveEquals_ComparesCaseInsensitive(string a, string b, bool expected)
        => Assert.Equal(expected, a.InsensitiveEquals(b));

    [Fact]
    public void InsensitiveEquals_OnlyOneNull_ReturnsFalse()
    {
        Assert.False("hello".InsensitiveEquals(null));
        Assert.False(((string?)null).InsensitiveEquals("hello"));
    }

    [Fact]
    public void InsensitiveIndexOf_FindsIgnoreCase()
        => Assert.Equal(6, "Hello WORLD".InsensitiveIndexOf("world"));

    [Fact]
    public void InsensitiveIndexOf_NullReceiver_ReturnsMinusOne()
        => Assert.Equal(-1, ((string?)null).InsensitiveIndexOf("x"));

    [Fact]
    public void InsensitiveRemove_RemovesAllMatches()
    {
        // "abAcAdAB" → remove case-insensitive "ab" at positions 0 and 6 → "AcAd"
        var result = "abAcAdAB".AsSpan().InsensitiveRemove("ab".AsSpan());
        Assert.Equal("AcAd", result);
    }

    [Fact]
    public void InsensitiveReplace_ReplacesIgnoreCase()
        => Assert.Equal("hi WORLD", "Hello WORLD".InsensitiveReplace("hello", "hi"));

    [Theory, InlineData("Hello", "HE", true), InlineData("Hello", "he", true), InlineData("Hello", "lo", false)]
    public void InsensitiveStartsWith_ChecksPrefix(string a, string b, bool expected)
        => Assert.Equal(expected, a.InsensitiveStartsWith(b));
}
