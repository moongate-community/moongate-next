using DryIoc;
using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;

namespace Moongate.PluginFixtures.Multiple;

public sealed class FirstPlugin : IMoongatePlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "moongate.fixture.first",
        Name = "First Fixture Plugin",
        Version = new Version(1, 0, 0),
        Author = "Moongate Tests"
    };

    public void Configure(IContainer container, PluginContext context)
    {
    }
}

public sealed class SecondPlugin : IMoongatePlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "moongate.fixture.second",
        Name = "Second Fixture Plugin",
        Version = new Version(1, 0, 0),
        Author = "Moongate Tests"
    };

    public void Configure(IContainer container, PluginContext context)
    {
    }
}
