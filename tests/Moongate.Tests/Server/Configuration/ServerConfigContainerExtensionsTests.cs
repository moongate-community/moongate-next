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
        File.WriteAllText(ConfigPath, "server:\n  server_name: \"Custom Shard\"\n  is_registration_allowed: true\n");

        var container = new Container();
        container.AddMoongateServerConfig();
        container.AddMoongateConfig(ConfigPath);

        var config = container.Resolve<ServerConfig>();

        Assert.Equal("Custom Shard", config.ServerName);
        Assert.True(config.IsRegistrationAllowed);
    }

    [Fact]
    public void AddMoongateServerConfig_MissingSection_UsesDefault()
    {
        var container = new Container();
        container.AddMoongateServerConfig();
        container.AddMoongateConfig(ConfigPath);

        var config = container.Resolve<ServerConfig>();

        Assert.Equal("Moongate Server", config.ServerName);
        Assert.False(config.IsRegistrationAllowed);
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
