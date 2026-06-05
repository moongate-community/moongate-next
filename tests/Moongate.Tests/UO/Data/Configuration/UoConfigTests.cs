using Moongate.UO.Data.Data;

namespace Moongate.Tests.UO.Data.Configuration;

public class UoConfigTests
{
    [Fact]
    public void Default_ClientFilesDirectory_IsHomeUo()
    {
        var config = new UoConfig();

        Assert.Equal("~/uo", config.ClientFilesDirectory);
    }

    [Fact]
    public void Validate_MissingDirectory_ReturnsError()
    {
        var config = new UoConfig
        {
            ClientFilesDirectory = Path.Combine(Path.GetTempPath(), "nr-uo-does-not-exist-" + Guid.NewGuid().ToString("N"))
        };

        var errors = config.Validate().ToList();

        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Validate_DirectoryWithoutTileData_ReturnsError()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var config = new UoConfig { ClientFilesDirectory = dir.FullName };

            var errors = config.Validate().ToList();

            Assert.Contains(errors, e => e.Contains("tiledata.mul"));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void Validate_DirectoryWithTileData_IsValid()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            File.WriteAllBytes(Path.Combine(dir.FullName, "tiledata.mul"), [0]);
            var config = new UoConfig { ClientFilesDirectory = dir.FullName };

            Assert.Empty(config.Validate());
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void Default_StartingLocation_IsTrammelBritain()
    {
        var config = new UoConfig();

        Assert.Equal(1, config.StartingMapId);
        Assert.Equal(1496, config.StartingX);
        Assert.Equal(1628, config.StartingY);
        Assert.Equal(10, config.StartingZ);
        Assert.Equal("Britain", config.StartingCity);
    }

    [Fact]
    public void Validate_StartingMapOutOfRange_ReturnsError()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            File.WriteAllBytes(Path.Combine(dir.FullName, "tiledata.mul"), [0]);
            var config = new UoConfig { ClientFilesDirectory = dir.FullName, StartingMapId = 9 };

            Assert.Contains(config.Validate(), e => e.Contains("starting"));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void Validate_StartingMapInRange_IsValid()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            File.WriteAllBytes(Path.Combine(dir.FullName, "tiledata.mul"), [0]);
            var config = new UoConfig { ClientFilesDirectory = dir.FullName, StartingMapId = 0 };

            Assert.Empty(config.Validate());
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
