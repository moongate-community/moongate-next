using DryIoc;
using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;

namespace Moongate.Tests.Plugins.Support;

public sealed class FakePlugin : IMoongatePlugin
{
    public FakePlugin(string id, params string[] dependencies)
    {
        Metadata = new PluginMetadata
        {
            Id = id,
            Name = id,
            Version = new Version(1, 0, 0),
            Author = "Moongate Tests",
            Dependencies = dependencies
        };
    }

    public PluginMetadata Metadata { get; }

    public void Configure(IContainer container, PluginContext context)
    {
    }
}
