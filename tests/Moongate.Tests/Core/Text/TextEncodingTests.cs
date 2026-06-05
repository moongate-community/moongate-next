using System.Text;
using Moongate.Core.Text;

namespace Moongate.Tests.Core.Text;

public class TextEncodingTests
{
    [Fact]
    public void GetByteLengthForEncoding_Utf16_ReturnsTwo()
    {
        Assert.Equal(2, Encoding.Unicode.GetByteLengthForEncoding());
        Assert.Equal(2, Encoding.BigEndianUnicode.GetByteLengthForEncoding());
    }

    [Fact]
    public void GetByteLengthForEncoding_Utf32_ReturnsFour()

        // Regression: UTF-32 was previously reported as 3 bytes, breaking buffer sizing.
        => Assert.Equal(4, Encoding.UTF32.GetByteLengthForEncoding());

    [Fact]
    public void GetByteLengthForEncoding_Utf8_ReturnsOne()
    {
        Assert.Equal(1, Encoding.UTF8.GetByteLengthForEncoding());
        Assert.Equal(1, Encoding.ASCII.GetByteLengthForEncoding());
    }

    [Fact]
    public void GetBytesUtf8_EmptyString_ReturnsEmptyArray()
        => Assert.Empty("".GetBytesUtf8());

    [Fact]
    public void GetBytesUtf8_Span_WritesIntoBuffer()
    {
        Span<byte> buffer = stackalloc byte[16];
        var written = "abc".GetBytesUtf8(buffer);

        Assert.Equal(3, written);
        Assert.Equal((byte)'a', buffer[0]);
        Assert.Equal((byte)'b', buffer[1]);
        Assert.Equal((byte)'c', buffer[2]);
    }

    [Fact]
    public void GetBytesUtf8_String_RoundtripsThroughDecode()
    {
        var bytes = "hello".GetBytesUtf8();
        Assert.Equal("hello", Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public void GetString_SafeStringFalse_DecodesAllBytes()
    {
        var bytes = Encoding.UTF8.GetBytes("hello");
        var result = TextEncoding.GetString(bytes, Encoding.UTF8);

        Assert.Equal("hello", result);
    }

    [Fact]
    public void GetString_SafeStringTrue_FiltersControlChars()
    {
        // 0x00 (NUL) is outside the [0x20, 0xFFFD] printable range and must be filtered.
        var bytes = Encoding.UTF8.GetBytes("a\0b\0c");
        var result = TextEncoding.GetString(bytes, Encoding.UTF8, true);

        Assert.Equal("abc", result);
    }

    [Fact]
    public void GetString_SafeStringTrue_PrintableBytesPassThrough()
    {
        var bytes = Encoding.UTF8.GetBytes("printable!");
        var result = TextEncoding.GetString(bytes, Encoding.UTF8, true);

        Assert.Equal("printable!", result);
    }

    [Fact]
    public void StaticEncodings_AreSingletons()
    {
        Assert.Same(TextEncoding.UTF8, TextEncoding.UTF8);
        Assert.Same(TextEncoding.Unicode, TextEncoding.Unicode);
        Assert.Same(TextEncoding.UnicodeLE, TextEncoding.UnicodeLE);
    }
}
