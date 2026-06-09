using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Scripting.Lua.Data.Config;
using Moongate.Scripting.Lua.Data.Internal;
using Moongate.Scripting.Lua.Services;

namespace Moongate.Tests.Scripting.Lua.Support;

/// <summary>
/// Builds a real <see cref="LuaScriptEngineService" /> backed by a throwaway temp scripts directory
/// and a DryIoc container, and cleans both up on dispose. Used by the Lua engine unit tests.
/// </summary>
internal sealed class LuaEngineFixture : IDisposable
{
    private readonly Container _container;

    public LuaEngineFixture(IEnumerable<ScriptModuleData>? modules = null, Action<IContainer>? configure = null)
    {
        ScriptsDirectory = Path.Combine(Path.GetTempPath(), $"nh-lua-{Guid.NewGuid():N}");
        Directory.CreateDirectory(ScriptsDirectory);

        _container = new();
        configure?.Invoke(_container);

        var directoriesConfig = new DirectoriesConfig(ScriptsDirectory, Array.Empty<string>());
        var config = new LuaEngineConfig(ScriptsDirectory, ScriptsDirectory, "test");

        Engine = new(
            directoriesConfig,
            _container,
            config,
            modules?.ToList() ?? [],
            []
        );
    }

    public LuaScriptEngineService Engine { get; }

    public IContainer Container => _container;

    public string ScriptsDirectory { get; }

    public void Dispose()
    {
        Engine.Dispose();
        _container.Dispose();

        if (Directory.Exists(ScriptsDirectory))
        {
            DeleteDirectoryWithRetry(ScriptsDirectory);
        }
    }

    private static void DeleteDirectoryWithRetry(string directory)
    {
        const int maxAttempts = 5;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Directory.Delete(directory, true);

                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(50);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(50);
            }
        }
    }
}
