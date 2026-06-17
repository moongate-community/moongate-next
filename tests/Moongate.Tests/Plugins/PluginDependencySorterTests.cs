using Moongate.Plugins.Data;
using Moongate.Plugins.Internal;
using Moongate.Tests.Plugins.Support;

namespace Moongate.Tests.Plugins;

public class PluginDependencySorterTests
{
    [Fact]
    public void ValidateAndSort_Cycle_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => PluginDependencySorter.ValidateAndSort(
                [
                    Loaded("moongate.a", "moongate.b"),
                    Loaded("moongate.b", "moongate.a")
                ]
            )
        );

        Assert.Contains("cycle", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateAndSort_DependentPlugin_ReturnsDependencyFirst()
    {
        var dependent = Loaded("moongate.dependent", "moongate.dependency");
        var dependency = Loaded("moongate.dependency");

        var sorted = PluginDependencySorter.ValidateAndSort([dependent, dependency]);

        Assert.Equal(
            ["moongate.dependency", "moongate.dependent"],
            sorted.Select(p => p.Metadata.Id).ToArray()
        );
    }

    [Fact]
    public void ValidateAndSort_DuplicateId_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            PluginDependencySorter.ValidateAndSort([Loaded("moongate.duplicate"), Loaded("moongate.duplicate")])
        );

        Assert.Contains("Duplicate plugin id", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Moongate.Bad")]
    [InlineData("moongate bad")]
    public void ValidateAndSort_InvalidId_Throws(string id)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => PluginDependencySorter.ValidateAndSort([Loaded(id)]));

        Assert.Contains("plugin id", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateAndSort_MissingDependency_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            PluginDependencySorter.ValidateAndSort([Loaded("moongate.dependent", "moongate.missing")])
        );

        Assert.Contains("missing dependency", ex.Message);
    }

    private static LoadedPlugin Loaded(string id, params string[] dependencies)
    {
        return new LoadedPlugin(
            Path.Combine(Path.GetTempPath(), $"nh-plugin-{Guid.NewGuid():N}"),
            new FakePlugin(id, dependencies),
            typeof(FakePlugin).Assembly
        );
    }
}
