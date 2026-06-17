using DryIoc;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Server.Extensions.Configuration;
using Moongate.Tests.Hosting.Configuration.Support;

namespace Moongate.Tests.Hosting.Configuration;

public class ConfigContainerExtensionsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-config-di-{Guid.NewGuid():N}");
    private string Path_ => Path.Combine(_dir, "moongate.yaml");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void AddMoongateConfig_MissingFile_CreatesDefaultAndRegistersDefault()
    {
        var container = new Container();
        container.RegisterConfigSection("server", () => new TestServerSettings());

        container.AddMoongateConfig(Path_);

        Assert.True(File.Exists(Path_));
        Assert.Equal(2593, container.Resolve<TestServerSettings>().Port);
    }

    [Fact]
    public void AddMoongateConfig_NoSections_CreatesNothingAndDoesNotThrow()
    {
        var container = new Container();

        container.AddMoongateConfig(Path_);

        Assert.False(File.Exists(Path_));
    }

    [Fact]
    public void AddMoongateConfig_RegistersBoundInstance()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "server:\n  port: 9000\n");

        var container = new Container();
        container.RegisterConfigSection("server", () => new TestServerSettings());
        container.AddMoongateConfig(Path_);

        Assert.Equal(9000, container.Resolve<TestServerSettings>().Port);
    }
}
