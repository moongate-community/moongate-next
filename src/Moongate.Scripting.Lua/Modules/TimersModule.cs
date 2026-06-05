using System.Collections.Concurrent;
using System.Globalization;
using MoonSharp.Interpreter;
using Moongate.Abstractions.Interfaces.Timing;
using Moongate.Scripting.Lua.Attributes.Scripts;
using Moongate.Scripting.Lua.Interfaces.Events;

namespace Moongate.Scripting.Lua.Modules;

[ScriptModule("timers", "Provides timer helpers to Lua scripts.")]
public sealed class TimersModule : IDisposable
{
    private readonly ILuaEventBridge _events;
    private readonly ConcurrentDictionary<string, string> _timers = new(StringComparer.Ordinal);
    private readonly ITimerService? _timer;

    public TimersModule(ILuaEventBridge events, ITimerService? timer = null)
    {
        _events = events;
        _timer = timer;
    }

    [ScriptFunction("cancel", "Cancels a timer by Lua timer name.")]
    public bool Cancel(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (_timer is null)
        {
            return false;
        }

        if (_timers.TryRemove(name, out var timerId))
        {
            return _timer.UnregisterTimer(timerId);
        }

        return _timer.UnregisterTimersByName(name) > 0;
    }

    public void Dispose()
    {
        if (_timer is not null)
        {
            foreach (var (_, timerId) in _timers)
            {
                _timer.UnregisterTimer(timerId);
            }
        }

        _timers.Clear();
    }

    [ScriptFunction("every", "Registers a repeating timer.")]
    public string Every(string name, string interval, Closure callback)
        => Register(name, interval, callback, true);

    [ScriptFunction("once", "Registers a one-shot timer.")]
    public string Once(string name, string interval, Closure callback)
        => Register(name, interval, callback, false);

    private static TimeSpan ParseInterval(string interval)
    {
        if (TimeSpan.TryParse(interval, CultureInfo.InvariantCulture, out var parsed) && parsed > TimeSpan.Zero)
        {
            return parsed;
        }

        throw new ScriptRuntimeException($"Invalid timer interval '{interval}'.");
    }

    private string Register(string name, string interval, Closure callback, bool repeat)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(interval);
        ArgumentNullException.ThrowIfNull(callback);

        if (_timer is null)
        {
            throw new ScriptRuntimeException("Timer service is not registered.");
        }

        var parsedInterval = ParseInterval(interval);
        var timerId = _timer.RegisterTimer(
            name,
            parsedInterval,
            () => _events.Invoke(
                callback,
                new Dictionary<string, object?>
                {
                    ["name"] = name,
                    ["repeat"] = repeat
                }
            ),
            repeat: repeat
        );

        _timers[name] = timerId;

        return timerId;
    }
}
