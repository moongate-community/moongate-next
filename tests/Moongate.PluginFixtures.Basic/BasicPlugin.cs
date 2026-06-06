using DryIoc;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;
using Moongate.Scripting.Lua.Attributes.Scripts;
using Moongate.Scripting.Lua.Extensions.Scripts;

namespace Moongate.PluginFixtures.Basic;

public sealed class BasicPlugin : IMoongatePlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "moongate.fixture.basic",
        Name = "Basic Fixture Plugin",
        Version = new(1, 0, 0),
        Author = "Moongate Tests"
    };

    public void Configure(IContainer container, PluginContext context)
    {
        context.LoadConfig(() => new BasicPluginYamlConfig());
        container.RegisterConfigSection("fixture_plugin", () => new BasicPluginServerConfig());
        container.RegisterScriptModule<BasicPluginScriptModule>();
    }
}

public sealed class BasicPluginYamlConfig
{
    public int WeatherIntervalSeconds { get; set; } = 2;
}

public sealed class BasicPluginServerConfig
{
    public string Message { get; set; } = "hello from fixture";
}

[ScriptModule("fixture_basic", "Fixture plugin script module.")]
public sealed class BasicPluginScriptModule;
