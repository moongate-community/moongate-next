using System.Collections.Concurrent;
using MoonSharp.Interpreter;
using Moongate.Scripting.Lua.Interfaces.Events;
using Serilog;

namespace Moongate.Scripting.Lua.Services;

/// <summary>
/// Default Lua event bridge backed by named MoonSharp closures.
/// </summary>
public sealed class LuaEventBridge : ILuaEventBridge
{
    private readonly ConcurrentDictionary<string, List<Closure>> _callbacks = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger _logger = Log.ForContext<LuaEventBridge>();

    private Script? _script;

    public void Attach(Script script)
    {
        ArgumentNullException.ThrowIfNull(script);

        _script = script;
    }

    public void Invoke(Closure callback, IReadOnlyDictionary<string, object?> payload)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ArgumentNullException.ThrowIfNull(payload);

        var script = _script ?? throw new InvalidOperationException("Lua event bridge is not attached to a script.");
        var table = CreatePayloadTable(script, payload);

        script.Call(callback, table);
    }

    public void Publish(string eventName, IReadOnlyDictionary<string, object?> payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        ArgumentNullException.ThrowIfNull(payload);

        if (!_callbacks.TryGetValue(eventName, out var callbacks))
        {
            return;
        }

        Closure[] snapshot;

        lock (callbacks)
        {
            snapshot = callbacks.ToArray();
        }

        for (var i = 0; i < snapshot.Length; i++)
        {
            try
            {
                Invoke(snapshot[i], payload);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lua event callback failed for {EventName}", eventName);
            }
        }
    }

    public void Register(string eventName, Closure callback)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        ArgumentNullException.ThrowIfNull(callback);

        var callbacks = _callbacks.GetOrAdd(eventName, static _ => []);

        lock (callbacks)
        {
            callbacks.Add(callback);
        }
    }

    private static DynValue ConvertValue(Script script, object? value)
    {
        if (value is null)
        {
            return DynValue.Nil;
        }

        if (value is IReadOnlyDictionary<string, object?> dictionary)
        {
            return DynValue.NewTable(CreatePayloadTable(script, dictionary));
        }

        return DynValue.FromObject(script, value);
    }

    private static Table CreatePayloadTable(Script script, IReadOnlyDictionary<string, object?> payload)
    {
        var table = new Table(script);

        foreach (var (key, value) in payload)
        {
            table[key] = ConvertValue(script, value);
        }

        return table;
    }
}
