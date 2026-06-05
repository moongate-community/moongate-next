using Moongate.Core.Buffers;

namespace Moongate.Tests.Core.Buffers;

public class RawInterpolatedStringHandlerTests
{
    [Fact]
    public void Handler_AppendFormatted_WithAlignment_PadsLeft()
    {
        var handler = new RawInterpolatedStringHandler(0, 1);
        handler.AppendFormatted("ab".AsSpan(), 5);

        Assert.Equal("   ab", handler.Text.ToString());
        handler.Clear();
    }

    [Fact]
    public void Handler_AppendFormatted_WithNegativeAlignment_PadsRight()
    {
        var handler = new RawInterpolatedStringHandler(0, 1);
        handler.AppendFormatted("ab".AsSpan(), -5);

        Assert.Equal("ab   ", handler.Text.ToString());
        handler.Clear();
    }

    [Fact]
    public void Handler_AppendFormattedReadOnlySpan_WritesSpan()
    {
        var handler = new RawInterpolatedStringHandler(0, 1);
        handler.AppendFormatted("xyz".AsSpan());

        Assert.Equal("xyz", handler.Text.ToString());
        handler.Clear();
    }

    [Fact]
    public void Handler_AppendFormattedString_WritesString()
    {
        var handler = new RawInterpolatedStringHandler(0, 1);
        handler.AppendFormatted("abc");

        Assert.Equal("abc", handler.Text.ToString());
        handler.Clear();
    }

    [Fact]
    public void Handler_AppendLiteralAndFormatted_ProducesExpectedText()
    {
        var handler = new RawInterpolatedStringHandler(7, 1);
        handler.AppendLiteral("value=");
        handler.AppendFormatted(42);

        Assert.Equal("value=42", handler.Text.ToString());
        handler.Clear();
    }

    [Fact]
    public void Handler_Clear_ResetsState()
    {
        var handler = new RawInterpolatedStringHandler(0, 1);
        handler.AppendLiteral("temp");
        handler.Clear();

        Assert.Equal(0, handler.Text.Length);
    }

    [Fact]
    public void Handler_LargeAppend_GrowsBuffer()
    {
        var handler = new RawInterpolatedStringHandler(0, 1);
        var big = new string('a', 1024);
        handler.AppendLiteral(big);

        Assert.Equal(big, handler.Text.ToString());
        handler.Clear();
    }
}
