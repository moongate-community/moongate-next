using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Localization;

namespace Moongate.Tests.UO.Data.Localization;

public class LocalizationServiceTests
{
    [Fact]
    public void Load_EmptyTextEntry_DoesNotTruncateTable()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            ClilocFixture.Write(
                dir.FullName,
                [
                    new ClilocFixture.Entry(1, 0, "first"),
                    new ClilocFixture.Entry(2, 0, ""),
                    new ClilocFixture.Entry(3, 0, "third")
                ]
            );
            var service = new LocalizationService(new UoFileResolver(dir.FullName));

            Assert.Equal(3, service.Count);
            Assert.Equal("", service.GetText(2));
            Assert.Equal("third", service.GetText(3));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void Load_ParsesEntries_AndFormats()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            ClilocFixture.Write(
                dir.FullName,
                [
                    new ClilocFixture.Entry(1042971, 0, "~1_NOTHING~"),
                    new ClilocFixture.Entry(500000, 0, "a dagger")
                ]
            );
            var service = new LocalizationService(new UoFileResolver(dir.FullName));

            Assert.Equal(2, service.Count);
            Assert.Equal("a dagger", service.GetText(500000));
            Assert.Equal("hello", service.Format(1042971, "hello"));
            Assert.Null(service.GetText(999999));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void MissingFile_YieldsEmptyTable_NoThrow()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var service = new LocalizationService(new UoFileResolver(dir.FullName));

            Assert.Equal(0, service.Count);
            Assert.Null(service.GetText(1));
            Assert.Equal("", service.Format(1));
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
