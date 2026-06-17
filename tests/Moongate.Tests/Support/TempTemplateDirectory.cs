namespace Moongate.Tests.Support;

/// <summary>
///     Creates a unique temp directory for YAML template test files; deleted on dispose.
/// </summary>
public sealed class TempTemplateDirectory : IDisposable
{
    public TempTemplateDirectory()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "moongate-template-tests",
            Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, true);
        }
    }

    public void WriteFile(string fileName, string yaml)
    {
        var fullPath = System.IO.Path.Combine(Path, fileName);
        var directory = System.IO.Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, yaml);
    }
}
