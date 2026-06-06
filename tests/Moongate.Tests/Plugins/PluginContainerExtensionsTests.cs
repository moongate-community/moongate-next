using DryIoc;
using Moongate.Abstractions.Data.Internal;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Scripting.Lua.Data.Internal;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.Plugins;
using Moongate.Server.Extensions.Scripting;
using Moongate.Tests.Plugins.Support;

namespace Moongate.Tests.Plugins;

public sealed class PluginContainerExtensionsTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"nh-plugin-container-{Guid.NewGuid():N}");

    [Fact]
    public void AddMoongatePlugins_LoadsPluginsBeforeGlobalConfigBinding()
    {
        var directories = new DirectoriesConfig(_root, Enum.GetNames<DirectoryType>());
        PluginFixtureCopy.CopyFixture(directories[DirectoryType.Plugins], "Moongate.PluginFixtures.Basic", "basic");
        var container = new Container();
        container.AddMoongateLuaScripting(directories);

        container.AddMoongatePlugins(directories);
        var sections = container.Resolve<List<ConfigSectionRegistration>>();

        Assert.Contains(sections, section => section.Name == "fixture_plugin");
        Assert.Contains(
            container.Resolve<List<ScriptModuleData>>(),
            module => module.ModuleType.FullName == "Moongate.PluginFixtures.Basic.BasicPluginScriptModule"
        );

        var configPath = Path.Combine(directories[DirectoryType.Config], "moongate.yaml");
        container.AddMoongateConfig(configPath);
        Assert.Contains("fixture_plugin:", File.ReadAllText(configPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }
}
