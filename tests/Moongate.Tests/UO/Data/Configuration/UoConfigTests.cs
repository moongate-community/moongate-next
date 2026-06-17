using Moongate.Abstractions.Configuration;
using Moongate.Core.Geometry;
using Moongate.UO.Data.Data;
using Moongate.UO.Data.Types.Maps;

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
    public void Default_StartingLocation_IsTrammelBritain()
    {
        var config = new UoConfig();

        Assert.Equal(UoMapFacetType.Trammel, config.StartingMap);
        Assert.Equal(new Point3D(1496, 1628, 10), config.Starting);
        Assert.Equal("Britain", config.StartingCity);
    }

    [Fact]
    public void Deserialize_CompactStartingLocation_BindsMapAndPoint()
    {
        const string yaml = """
                            client_files_directory: ~/uo
                            starting_map: Trammel
                            starting: 1496,1628,10
                            starting_city: Britain
                            """;

        var config = ConfigYamlOptions.Deserializer.Deserialize<UoConfig>(yaml);

        Assert.NotNull(config);
        Assert.Equal(UoMapFacetType.Trammel, config.StartingMap);
        Assert.Equal(new Point3D(1496, 1628, 10), config.Starting);
    }

    [Fact]
    public void Serialize_DefaultStartingLocation_WritesCompactPoint()
    {
        var yaml = ConfigYamlOptions.Serializer.Serialize(new UoConfig());

        Assert.Contains("starting_map: Trammel", yaml);
        Assert.Contains("starting: 1496,1628,10", yaml);
        Assert.DoesNotContain("starting_map_id", yaml);
        Assert.DoesNotContain("starting_x", yaml);
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
    public void Validate_KnownStartingMap_IsValid()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            File.WriteAllBytes(Path.Combine(dir.FullName, "tiledata.mul"), [0]);
            var config = new UoConfig
            {
                ClientFilesDirectory = dir.FullName,
                StartingMap = UoMapFacetType.Felucca
            };

            Assert.Empty(config.Validate());
        }
        finally
        {
            dir.Delete(true);
        }
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
    public void Validate_UnknownStartingMap_ReturnsError()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            File.WriteAllBytes(Path.Combine(dir.FullName, "tiledata.mul"), [0]);
            var config = new UoConfig
            {
                ClientFilesDirectory = dir.FullName,
                StartingMap = (UoMapFacetType)9
            };

            Assert.Contains(config.Validate(), e => e.Contains("starting"));
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
