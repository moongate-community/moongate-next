using System.Collections.Concurrent;
using System.Globalization;
using Moongate.Abstractions.Interfaces.Jobs;
using Moongate.Abstractions.Types.Jobs;
using Moongate.Scripting.Lua.Attributes.Scripts;
using Moongate.Scripting.Lua.Interfaces.Events;
using Moongate.Scripting.Lua.Utils;
using MoonSharp.Interpreter;

namespace Moongate.Scripting.Lua.Modules;

[ScriptModule("jobs", "Registers named scheduled jobs visible in the admin UI.")]
public sealed class JobsModule : IDisposable
{
    private readonly ILuaEventBridge _events;
    private readonly ConcurrentDictionary<string, string> _jobs = new(StringComparer.Ordinal);
    private readonly IJobService? _service;

    public JobsModule(ILuaEventBridge events, IJobService? service = null)
    {
        _events = events;
        _service = service;
    }

    [ScriptFunction("every", "Registers a repeating job.")]
    public string Every(string name, string interval, Closure callback, string? description = null)
        => Register(name, interval, callback, repeat: true, description);

    [ScriptFunction("once", "Registers a one-shot job.")]
    public string Once(string name, string interval, Closure callback, string? description = null)
        => Register(name, interval, callback, repeat: false, description);

    [ScriptFunction("run_now", "Schedules an immediate run of a job by id.")]
    public bool RunNow(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return _service is not null && _service.RunNow(id);
    }

    [ScriptFunction("cancel", "Cancels a job by Lua job name.")]
    public bool Cancel(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (_service is null)
        {
            return false;
        }

        return _jobs.TryRemove(name, out var id) && _service.Cancel(id);
    }

    public void Dispose()
    {
        if (_service is not null)
        {
            foreach (var (_, id) in _jobs)
            {
                _service.Cancel(id);
            }
        }

        _jobs.Clear();
    }

    private static TimeSpan ParseInterval(string interval)
    {
        if (TimeSpan.TryParse(interval, CultureInfo.InvariantCulture, out var parsed) && parsed > TimeSpan.Zero)
        {
            return parsed;
        }

        throw new ScriptRuntimeException($"Invalid job interval '{interval}'.");
    }

    private string Register(string name, string interval, Closure callback, bool repeat, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(interval);
        ArgumentNullException.ThrowIfNull(callback);

        if (_service is null)
        {
            throw new ScriptRuntimeException("Job service is not registered.");
        }

        var parsed = ParseInterval(interval);

        var id = repeat
                     ? _service.RegisterRecurring(
                         name,
                         parsed,
                         () => _events.Invoke(callback, LuaPayloadBuilder.Timer(name, repeat)),
                         description,
                         source: JobSourceType.Lua
                     )
                     : _service.RegisterOnce(
                         name,
                         parsed,
                         () => _events.Invoke(callback, LuaPayloadBuilder.Timer(name, repeat)),
                         description,
                         source: JobSourceType.Lua
                     );

        _jobs[name] = id;

        return id;
    }
}
