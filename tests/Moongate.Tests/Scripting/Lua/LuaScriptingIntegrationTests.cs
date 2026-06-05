using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Scripting.Lua.Interfaces.Scripts;
using Moongate.Server.Extensions.Scripting;
using Moongate.Tests.Support;

namespace Moongate.Tests.Scripting.Lua;

public class LuaScriptingIntegrationTests
{
    [Fact]
    public void AddMoongateLuaScripting_RegistersEngineAndHostedService()
    {
        var scriptsDir = Path.Combine(Path.GetTempPath(), $"nh-lua-reg-{Guid.NewGuid():N}");
        Directory.CreateDirectory(scriptsDir);

        try
        {
            var directoriesConfig = new DirectoriesConfig(scriptsDir, Array.Empty<string>());

            var container = new Container();
            container.AddMoongateLuaScripting(directoriesConfig);

            Assert.NotNull(container.Resolve<IScriptEngineService>());
            Assert.NotNull(container.Orchestrator());
        }
        finally
        {
            if (Directory.Exists(scriptsDir))
            {
                Directory.Delete(scriptsDir, true);
            }
        }
    }

    [Fact]
    public async Task FullHost_OnDryIoc_StartsEngineAndRunsBootstrapScript()
    {
        var scriptsDir = Path.Combine(Path.GetTempPath(), $"nh-lua-int-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(scriptsDir, "scripts"));

        try
        {
            // A bootstrap.lua that writes a marker file proves the engine ran user scripts.
            var markerPath = Path.Combine(scriptsDir, "ran.txt");
            var luaMarker = markerPath.Replace("\\", "\\\\");
            await File.WriteAllTextAsync(
                Path.Combine(scriptsDir, "scripts", "bootstrap.lua"),
                $"local f = io.open(\"{luaMarker}\", \"w\"); f:write(\"ok\"); f:close()"
            );

            var directoriesConfig = new DirectoriesConfig(scriptsDir, Array.Empty<string>());

            var container = new Container();
            container.AddMoongateLuaScripting(directoriesConfig);

            var orchestrator = container.Orchestrator();
            var engine = container.Resolve<IScriptEngineService>();

            await orchestrator.StartAsync(CancellationToken.None);

            try
            {
                Assert.True(File.Exists(markerPath), "bootstrap.lua should have run and created the marker file");

                // Engine is live: a function call returns a result.
                var result = engine.ExecuteFunction("1 + 1");
                Assert.True(result.Success);
                Assert.Equal(2d, Assert.IsType<double>(result.Data));
            }
            finally
            {
                await orchestrator.StopAsync(CancellationToken.None);
            }
        }
        finally
        {
            if (Directory.Exists(scriptsDir))
            {
                Directory.Delete(scriptsDir, true);
            }
        }
    }
}
