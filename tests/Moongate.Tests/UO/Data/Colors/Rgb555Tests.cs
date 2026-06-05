using Moongate.UO.Data.Internal.Colors;

namespace Moongate.Tests.UO.Data.Colors;

public class Rgb555Tests
{
    [Fact]
    public void ToRgb_ExpandsChannels()
    {
        Assert.Equal(((byte)255, (byte)255, (byte)255), Rgb555.ToRgb(0x7FFF)); // all 5-bit max
        Assert.Equal(((byte)0, (byte)0, (byte)255), Rgb555.ToRgb(0x001F));     // blue max
        Assert.Equal(((byte)0, (byte)0, (byte)0), Rgb555.ToRgb(0x0000));       // black
    }
}
