using DryIoc;
using Moongate.Server.Data.Config;
using Moongate.Server.Extensions.Configuration;

namespace Moongate.Tests.Server.Configuration;

public sealed class ServerConfigContainerExtensionsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"mg-serverconfig-{Guid.NewGuid():N}");

    private string ConfigPath => Path.Combine(_dir, "moongate.yaml");

    [Fact]
    public void AddMoongateServerConfig_BindsServerName()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(ConfigPath, "server:\n  server_name: \"Custom Shard\"\n");

        var container = new Container();
        container.AddMoongateServerConfig();
        container.AddMoongateConfig(ConfigPath);

        Assert.Equal("Custom Shard", container.Resolve<ServerConfig>().ServerName);
    }

    [Fact]
    public void AddMoongateServerConfig_MissingSection_UsesDefault()
    {
        var container = new Container();
        container.AddMoongateServerConfig();
        container.AddMoongateConfig(ConfigPath);

        Assert.Equal("Moongate Server", container.Resolve<ServerConfig>().ServerName);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }
}
