using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Files.Internal;

namespace Moongate.Tests.UO.Data.Files;

public class UopIndexReaderTests
{
    [Fact]
    public void ReadIndexes_NonUopFile_Throws()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var path = Path.Combine(dir.FullName, "bad.uop");
            File.WriteAllBytes(path, [0, 0, 0, 0, 0, 0, 0, 0]);

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            Assert.Throws<FileLoadException>(() => UopIndexReader.ReadIndexes(fs, ".dat"));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void ReadIndexes_SingleEntry_ReturnsOffsetAndSize()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var (path, dataOffset, dataLength) =
                UopFixture.WriteSingleEntry(dir.FullName, "test", [1, 2, 3, 4, 5, 6, 7, 8]);

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var entries = UopIndexReader.ReadIndexes(fs, ".dat", 16, 5);

            Assert.True(entries.ContainsKey(0));
            Assert.Equal(dataOffset, entries[0].Offset);
            Assert.Equal(dataLength, entries[0].Size);
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
