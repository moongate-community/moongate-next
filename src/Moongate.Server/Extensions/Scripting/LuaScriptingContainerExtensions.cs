using DryIoc;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Core.Utils;
using Moongate.Scripting.Lua.Data.Config;
using Moongate.Scripting.Lua.Data.Internal;
using Moongate.Scripting.Lua.Extensions.Scripts;
using Moongate.Scripting.Lua.Interfaces.Events;
using Moongate.Scripting.Lua.Interfaces.Scripts;
using Moongate.Scripting.Lua.Modules;
using Moongate.Scripting.Lua.Services;
using Moongate.Server.Data.Events;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Services.Scripting;

namespace Moongate.Server.Extensions.Scripting;

/// <summary>
///     DryIoc-native registration helpers for the Moongate Lua scripting engine.
/// </summary>
public static class LuaScriptingContainerExtensions
{
    private const int LuaScriptingPriority = 30;

    /// <summary>
    ///     Registers the MoonSharp Lua <see cref="IScriptEngineService" /> and drives its lifecycle
    ///     through the Moongate hosting orchestrator. The engine resolves the DryIoc
    ///     <see cref="IContainer" /> itself to register and resolve script-module types at runtime.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="directoriesConfig">Resolved directories configuration.</param>
    public static IContainer AddMoongateLuaScripting(
        this IContainer container,
        DirectoriesConfig directoriesConfig
    )
    {
        ArgumentNullException.ThrowIfNull(directoriesConfig);

        container.AddMoongateHosting();

        var scriptsDirectory = directoriesConfig[DirectoryType.Scripts];
        var config = new LuaEngineConfig(scriptsDirectory, scriptsDirectory, VersionUtils.GetVersion());

        container.RegisterInstance(config);
        container.RegisterInstance(directoriesConfig, IfAlreadyRegistered.Keep);

        // Module/userdata accumulator lists the engine resolves from the container.
        container.RegisterInstance(new List<ScriptModuleData>(), IfAlreadyRegistered.Keep);
        container.RegisterInstance(new List<ScriptUserData>(), IfAlreadyRegistered.Keep);

        container.Register<ILuaEventBridge, LuaEventBridge>(Reuse.Singleton);

        container.Register<IScriptEngineService, LuaScriptEngineService>(Reuse.Singleton);
        container.AddMoongateService<LuaScriptHostedService>(LuaScriptingPriority);

        container.RegisterScriptModule<EventsModule>();
        container.RegisterScriptModule<CommandsModule>();
        container.RegisterScriptModule<LogModule>();
        container.RegisterScriptModule<RandomModule>();
        container.RegisterScriptModule<TimersModule>();
        container.RegisterScriptModule<JobsModule>();

        container.AddTickEventHandler<LuaServerStartedEventHandler, ServerStartedEvent>();
        container.AddTickEventHandler<LuaPlayerConnectedEventHandler, PlayerConnectedEvent>();
        container.AddTickEventHandler<LuaPlayerDisconnectedEventHandler, PlayerDisconnectedEvent>();

        return container;
    }
}
