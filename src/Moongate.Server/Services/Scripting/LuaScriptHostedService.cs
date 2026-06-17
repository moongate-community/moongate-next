using Moongate.Abstractions.Interfaces.Services;
using Moongate.Scripting.Lua.Interfaces.Scripts;

namespace Moongate.Server.Services.Scripting;

/// <summary>
///     Hosting adapter that drives the Lua <see cref="IScriptEngineService" /> lifecycle through the
///     Moongate service orchestrator. Keeps the scripting project free of any hosting dependency.
/// </summary>
public sealed class LuaScriptHostedService : IMoongateService
{
    private readonly IScriptEngineService _engine;

    public LuaScriptHostedService(IScriptEngineService engine)
    {
        _engine = engine;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _engine.StartAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_engine is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return Task.CompletedTask;
    }
}
