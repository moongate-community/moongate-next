using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Bootstrap;

namespace Moongate.Tests.Server.Bootstrap;

public sealed class BootstrapRegistrationExtensionsTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"moongate-bootstrap-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ResolveConfiguredWebBaseUrl_ReadsExistingWebSection()
    {
        var directories = new DirectoriesConfig(_root, Enum.GetNames<DirectoryType>());
        File.WriteAllText(
            Path.Combine(directories[DirectoryType.Config], "moongate.yaml"),
            """
            web:
              base_url: https://play.moongate.io
            """
        );

        var baseUrl = BootstrapRegistrationExtensions.ResolveConfiguredWebBaseUrl(directories);

        Assert.Equal("https://play.moongate.io", baseUrl);
    }
}
