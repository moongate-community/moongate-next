using Moongate.UO.Data.Internal.Compression;

namespace Moongate.Tests.UO.Data.Localization;

public class MythicDecompressTests
{
    [Fact]
    public void TransformDetransform_RoundTrips()
    {
        var input = new byte[] { 10, 20, 20, 30, 30, 30, 40, 0, 255, 7, 7, 1 };

        var transformed = MythicDecompress.Transform(input);
        var restored = MythicDecompress.Detransform(transformed);

        Assert.Equal(input, restored);
    }

    [Fact]
    public void MoveToFront_EncodeDecode_RoundTrips()
    {
        var input = new byte[] { 1, 2, 2, 3, 0, 255, 128, 128 };

        var encoded = MoveToFrontCoding.Encode(input);
        var decoded = MoveToFrontCoding.Decode(encoded);

        Assert.Equal(input, decoded);
    }
}
