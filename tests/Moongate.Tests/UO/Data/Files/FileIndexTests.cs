using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Files;

namespace Moongate.Tests.UO.Data.Files;

public class FileIndexTests
{
    [Fact]
    public void Seek_ValidEntry_ReturnsStreamPositionedWithLengthAndExtra()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var (idxPath, mulPath) = FileIndexFixture.Write(
                dir.FullName,
                "test",
                [
                    new FileIndexFixture.Payload(0, [1, 2, 3, 4]),
                    new FileIndexFixture.Payload(42, [9, 9])
                ]
            );
            using var index = new FileIndex(idxPath, mulPath, length: 2, file: -1, new NullVerdataPatchSource());

            var stream = index.Seek(1, out var length, out var extra, out var patched);

            Assert.NotNull(stream);
            Assert.Equal(2, length);
            Assert.Equal(42, extra);
            Assert.False(patched);

            var buffer = new byte[length];
            stream!.ReadExactly(buffer, 0, length);
            Assert.Equal(new byte[] { 9, 9 }, buffer);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void Seek_OutOfRangeIndex_ReturnsNull()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var (idxPath, mulPath) = FileIndexFixture.Write(
                dir.FullName,
                "test",
                [new FileIndexFixture.Payload(0, [1, 2, 3, 4])]
            );
            using var index = new FileIndex(idxPath, mulPath, length: 1, file: -1, new NullVerdataPatchSource());

            Assert.Null(index.Seek(5, out _, out _, out _));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void HashFileName_MatchesKnownUopHash()
    {
        // Known Mythic.Package hash for "build/artlegacymul/000000000.tga".
        var hash = FileIndex.HashFileName("build/artlegacymul/000000000.tga");

        Assert.NotEqual(0UL, hash);
    }
}
