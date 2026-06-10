using DryIoc;
using Moongate.Abstractions.Configuration;
using Moongate.Abstractions.Data.Logging;
using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Interfaces.Timing;
using Moongate.Abstractions.Internal;
using Moongate.Core.Types;
using Moongate.Network.UO.Registry;
using Moongate.Scripting.Lua.Interfaces.Scripts;
using Moongate.Server.Bootstrap;
using Moongate.Server.Data.Config;
using Moongate.Server.Interfaces.Auth;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.WorldData;
using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Data;

namespace Moongate.Tests.Server.Bootstrap;

public sealed class MoongateBootstrapTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"nr-bootstrap-{Guid.NewGuid():N}");

    [Fact]
    public void ConfigureContainer_RegistersCoreServicesAndConfig()
    {
        using var container = new Container();
        var context = MoongateBootstrap.CreateContext(new([], _root, false, false));
        WriteTestConfig(context);

        MoongateBootstrap.ConfigureContainer(container, context);

        Assert.Same(context.PacketRegistry, container.Resolve<PacketRegistry>());
        Assert.NotNull(container.Resolve<IEventBusService>());
        Assert.NotNull(container.Resolve<ITimerService>());
        Assert.NotNull(container.Resolve<IMetricsService>());
        Assert.NotNull(container.Resolve<IScriptEngineService>());
        Assert.NotNull(container.Resolve<IAuthTokenService>());
        Assert.NotNull(container.Resolve<IDoorDataService>());
        Assert.NotNull(container.Resolve<ISpawnsDataService>());
        Assert.NotNull(container.Resolve<ITeleportersDataService>());
        Assert.NotNull(container.Resolve<ServerAssetDataLoader>());
        Assert.NotNull(container.Resolve<ServerAssetDataBootService>());
        Assert.NotNull(container.Resolve<LoggerConfig>());
        Assert.NotNull(container.Resolve<WebConfig>());
        var configPath = Path.Combine(context.Directories[DirectoryType.Config], "moongate.yaml");
        Assert.True(File.Exists(configPath));
        var yaml = File.ReadAllText(configPath);
        Assert.Contains("web:", yaml);
        Assert.Contains("jwt:", yaml);

        var dataServices = container.ResolveMany<IDataService>().ToArray();
        Assert.Equal(12, dataServices.Length);
        Assert.All(
            dataServices,
            service =>
            {
                Assert.True(service.IsLazy);
                Assert.False(service.IsLoaded);
            }
        );

        var descriptor = container.Resolve<MoongateServiceDescriptor>(serviceKey: typeof(ServerAssetDataBootService));
        Assert.IsType<ServerAssetDataBootService>(descriptor.Service);
        Assert.Equal(11, descriptor.Priority);
    }

    [Fact]
    public void CreateContext_ResolvesDirectoriesAndRegistersPackets()
    {
        var options = new MoongateBootstrapOptions([], _root, false, false);

        var context = MoongateBootstrap.CreateContext(options);

        Assert.Equal(_root, context.Directories.Root);
        Assert.True(context.RegisteredPacketCount > 0);
        Assert.NotNull(context.PacketRegistry);
        Assert.True(Directory.Exists(context.Directories[DirectoryType.Config]));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Options_ExposeCliValues()
    {
        var args = new[] { "--debug" };
        var options = new MoongateBootstrapOptions(args, _root, true, false);

        Assert.Same(args, options.Args);
        Assert.True(options.Debug);
        Assert.False(options.ShowHeader);
        Assert.Equal(_root, options.RootDirectory);
    }

    private void WriteTestConfig(MoongateBootstrapContext context)
    {
        var uoDirectory = Path.Combine(_root, "uo-client");
        Directory.CreateDirectory(uoDirectory);
        TileDataFixture.Write(uoDirectory, [], []);

        var configPath = Path.Combine(context.Directories[DirectoryType.Config], "moongate.yaml");
        var config = new Dictionary<string, object?>
        {
            ["uo"] = new UoConfig
            {
                ClientFilesDirectory = uoDirectory
            }
        };

        File.WriteAllText(configPath, ConfigYamlOptions.Serializer.Serialize(config));
    }
}
