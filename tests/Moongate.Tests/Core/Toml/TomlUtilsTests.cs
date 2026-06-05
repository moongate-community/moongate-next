using Moongate.Core.Toml;
using Tomlyn;
using Tomlyn.Serialization;

namespace Moongate.Tests.Core.Toml;

public class TomlUtilsTests : IDisposable
{
    private readonly string _tempDir;

    public TomlUtilsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"nh-toml-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void Deserialize_InvalidToml_Throws()
        => Assert.ThrowsAny<Exception>(
            () => TomlUtils.Deserialize("this is = not valid = toml", TomlTestContext.Default.TomlPerson)
        );

    [Fact]
    public void Deserialize_NullContext_Throws()
        => Assert.Throws<ArgumentNullException>(
            () => TomlUtils.Deserialize<TomlPerson>("name = \"x\"", (TomlSerializerContext)null!)
        );

    [Fact]
    public void Deserialize_NullOrWhitespaceToml_Throws()
    {
        // ArgumentException.ThrowIfNullOrWhiteSpace throws ArgumentNullException for null
        // and ArgumentException for empty/whitespace.
        Assert.Throws<ArgumentNullException>(() => TomlUtils.Deserialize(null!, TomlTestContext.Default.TomlPerson));
        Assert.ThrowsAny<ArgumentException>(() => TomlUtils.Deserialize("", TomlTestContext.Default.TomlPerson));
        Assert.ThrowsAny<ArgumentException>(() => TomlUtils.Deserialize("   ", TomlTestContext.Default.TomlPerson));
    }

    [Fact]
    public void Deserialize_NullTypeInfo_Throws()
        => Assert.Throws<ArgumentNullException>(
            () => TomlUtils.Deserialize("name = \"x\"", (TomlTypeInfo<TomlPerson>)null!)
        );

    [Fact]
    public void DeserializeFromFile_MissingFile_ThrowsFileNotFound()
    {
        var missing = Path.Combine(_tempDir, "does-not-exist.toml");

        Assert.Throws<FileNotFoundException>(
            () => TomlUtils.DeserializeFromFile(missing, TomlTestContext.Default.TomlPerson)
        );
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void GetTomlContexts_ReturnsReadOnlySnapshot()
    {
        var contexts = TomlUtils.GetTomlContexts();

        Assert.IsAssignableFrom<IReadOnlyList<TomlSerializerContext>>(contexts);
    }

    [Fact]
    public void RegisterTomlContext_ContextAppearsInRegistry()
    {
        var ctx = TomlTestContext.Default;

        TomlUtils.RegisterTomlContext(ctx);
        var contexts = TomlUtils.GetTomlContexts();

        Assert.Contains(ctx, contexts);
    }

    [Fact]
    public void RegisterTomlContext_NullContext_Throws()
        => Assert.Throws<ArgumentNullException>(() => TomlUtils.RegisterTomlContext(null!));

    [Fact]
    public void Serialize_NullObject_Throws()
        => Assert.Throws<ArgumentNullException>(
            () => TomlUtils.Serialize<TomlPerson>(null!, TomlTestContext.Default.TomlPerson)
        );

    [Fact]
    public void Serialize_WithContext_RoundTripsThroughDeserialize()
    {
        var original = new TomlPerson { Name = "Aragorn", Age = 87 };

        var toml = TomlUtils.Serialize(original, TomlTestContext.Default);
        var deserialized = TomlUtils.Deserialize<TomlPerson>(toml, TomlTestContext.Default);

        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.Age, deserialized.Age);
    }

    [Fact]
    public void Serialize_WithTypeInfo_RoundTripsThroughDeserialize()
    {
        var original = new TomlPerson { Name = "Gandalf", Age = 2019 };

        var toml = TomlUtils.Serialize(original, TomlTestContext.Default.TomlPerson);
        var deserialized = TomlUtils.Deserialize(toml, TomlTestContext.Default.TomlPerson);

        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.Age, deserialized.Age);
    }

    [Fact]
    public void SerializeToFile_CreatesMissingDirectory()
    {
        var original = new TomlPerson { Name = "Sam", Age = 38 };
        var nestedPath = Path.Combine(_tempDir, "nested", "deeper", "person.toml");

        TomlUtils.SerializeToFile(original, nestedPath, TomlTestContext.Default.TomlPerson);

        Assert.True(File.Exists(nestedPath));
    }

    [Fact]
    public void SerializeToFile_EmptyPath_Throws()
    {
        var original = new TomlPerson { Name = "x", Age = 1 };

        Assert.ThrowsAny<ArgumentException>(
            () => TomlUtils.SerializeToFile(original, "", TomlTestContext.Default.TomlPerson)
        );
    }

    [Fact]
    public void SerializeToFile_NullObject_Throws()
    {
        var filePath = Path.Combine(_tempDir, "null.toml");

        Assert.Throws<ArgumentNullException>(
            () => TomlUtils.SerializeToFile(null!, filePath, TomlTestContext.Default.TomlPerson)
        );
    }

    [Fact]
    public void SerializeToFile_RoundTripsViaDeserializeFromFile()
    {
        var original = new TomlPerson { Name = "Frodo", Age = 50 };
        var filePath = Path.Combine(_tempDir, "person.toml");

        TomlUtils.SerializeToFile(original, filePath, TomlTestContext.Default.TomlPerson);

        Assert.True(File.Exists(filePath));

        var deserialized = TomlUtils.DeserializeFromFile(filePath, TomlTestContext.Default.TomlPerson);

        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.Age, deserialized.Age);
    }
}
