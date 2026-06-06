using Moongate.Core.Yaml;
using YamlDotNet.Core;

namespace Moongate.Tests.Core.Yaml;

public class YamlUtilsTests : IDisposable
{
    private readonly string _tempDir;

    public YamlUtilsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"moongate-yaml-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void Deserialize_InvalidYaml_Throws()
    {
        Assert.ThrowsAny<YamlException>(() => YamlUtils.Deserialize<YamlPerson>("name: ["));
    }

    [Fact]
    public void Deserialize_NullOrWhitespaceYaml_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => YamlUtils.Deserialize<YamlPerson>(null!));
        Assert.ThrowsAny<ArgumentException>(() => YamlUtils.Deserialize<YamlPerson>(""));
        Assert.ThrowsAny<ArgumentException>(() => YamlUtils.Deserialize<YamlPerson>("   "));
    }

    [Fact]
    public void SerializeAndDeserialize_RoundTripsSnakeCaseYaml()
    {
        var original = new YamlPerson { Name = "Sam", Age = 38, Delay = TimeSpan.FromMinutes(5) };

        var yaml = YamlUtils.Serialize(original);
        var deserialized = YamlUtils.Deserialize<YamlPerson>(yaml);

        Assert.Contains("delay: 00:05:00", yaml);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.Age, deserialized.Age);
        Assert.Equal(original.Delay, deserialized.Delay);
    }

    [Fact]
    public void SerializeToFile_CreatesNestedDirectories()
    {
        var original = new YamlPerson { Name = "Frodo", Age = 50, Delay = TimeSpan.FromSeconds(30) };
        var path = Path.Combine(_tempDir, "nested", "deeper", "person.yaml");

        YamlUtils.SerializeToFile(original, path);
        var deserialized = YamlUtils.DeserializeFromFile<YamlPerson>(path);

        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.Delay, deserialized.Delay);
    }

    [Fact]
    public void DeserializeFromFile_MissingFile_Throws()
    {
        var missing = Path.Combine(_tempDir, "missing.yaml");

        Assert.Throws<FileNotFoundException>(() => YamlUtils.DeserializeFromFile<YamlPerson>(missing));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }

        GC.SuppressFinalize(this);
    }
}
