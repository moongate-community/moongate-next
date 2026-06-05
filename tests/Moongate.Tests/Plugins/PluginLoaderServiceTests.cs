using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Plugins.Services;
using Moongate.Scripting.Lua.Data.Internal;
using Moongate.Server.Extensions.Scripting;
using Moongate.Tests.Plugins.Support;

namespace Moongate.Tests.Plugins;

public sealed class PluginLoaderServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"nh-plugin-loader-{Guid.NewGuid():N}");

    private string PluginsRoot => Path.Combine(_root, "plugins");

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void LoadAndConfigure_EmptyPluginDirectory_Throws()
    {
        PluginFixtureCopy.CopyFixture(PluginsRoot, "Moongate.PluginFixtures.Empty", "empty");
        var loader = new PluginLoaderService();

        var ex = Assert.Throws<InvalidOperationException>(() => loader.LoadAndConfigure(new Container(), Directories()));

        Assert.Contains("does not contain a plugin", ex.Message);
    }

    [Fact]
    public void LoadAndConfigure_MissingPluginsDirectory_CreatesDirectoryAndReturnsEmpty()
    {
        var container = new Container();
        var directories = Directories();
        var loader = new PluginLoaderService();

        var loaded = loader.LoadAndConfigure(container, directories);

        Assert.Empty(loaded);
        Assert.True(Directory.Exists(PluginsRoot));
    }

    [Fact]
    public void LoadAndConfigure_MultiplePluginImplementations_Throws()
    {
        PluginFixtureCopy.CopyFixture(PluginsRoot, "Moongate.PluginFixtures.Multiple", "multiple");
        var loader = new PluginLoaderService();

        var ex = Assert.Throws<InvalidOperationException>(() => loader.LoadAndConfigure(new Container(), Directories()));

        Assert.Contains("multiple plugin implementations", ex.Message);
    }

    [Fact]
    public void LoadAndConfigure_ValidPlugin_LoadsMetadataAndConfiguresContainer()
    {
        PluginFixtureCopy.CopyFixture(PluginsRoot, "Moongate.PluginFixtures.Basic", "basic");
        var container = new Container();
        container.AddMoongateLuaScripting(Directories());
        var loader = new PluginLoaderService();

        var loaded = loader.LoadAndConfigure(container, Directories());

        var plugin = Assert.Single(loaded);
        Assert.Equal("moongate.fixture.basic", plugin.Metadata.Id);
        Assert.True(File.Exists(Path.Combine(plugin.PluginDirectory, "plugin.toml")));
        var modules = container.Resolve<List<ScriptModuleData>>();
        Assert.Contains(
            modules,
            module => module.ModuleType.FullName == "Moongate.PluginFixtures.Basic.BasicPluginScriptModule"
        );
    }

    private DirectoriesConfig Directories()
        => new(_root, Enum.GetNames<DirectoryType>());
}
