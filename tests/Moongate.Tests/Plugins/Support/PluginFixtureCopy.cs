namespace Moongate.Tests.Plugins.Support;

public static class PluginFixtureCopy
{
    public static string CopyFixture(string pluginsRoot, string assemblyName, string? directoryName = null)
    {
        var source = Path.Combine(AppContext.BaseDirectory, assemblyName + ".dll");

        if (!File.Exists(source))
        {
            throw new FileNotFoundException($"Fixture assembly '{assemblyName}' was not copied to test output.", source);
        }

        var pluginDirectory = Path.Combine(pluginsRoot, directoryName ?? assemblyName);
        Directory.CreateDirectory(pluginDirectory);
        File.Copy(source, Path.Combine(pluginDirectory, assemblyName + ".dll"), true);

        return pluginDirectory;
    }
}
