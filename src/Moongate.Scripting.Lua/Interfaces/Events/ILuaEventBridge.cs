using MoonSharp.Interpreter;

namespace Moongate.Scripting.Lua.Interfaces.Events;

/// <summary>
/// Bridges named server events and internal callbacks into Lua closures.
/// </summary>
public interface ILuaEventBridge
{
    /// <summary>
    /// Attaches the active MoonSharp script runtime used to invoke registered closures.
    /// </summary>
    /// <param name="script">Active Lua script runtime.</param>
    void Attach(Script script);

    /// <summary>
    /// Invokes a single closure with the supplied payload.
    /// </summary>
    /// <param name="callback">Lua closure to invoke.</param>
    /// <param name="payload">Payload exposed to Lua as a table.</param>
    void Invoke(Closure callback, IReadOnlyDictionary<string, object?> payload);

    /// <summary>
    /// Publishes a named event to every Lua callback registered for it.
    /// </summary>
    /// <param name="eventName">Stable event name, such as <c>player.connected</c>.</param>
    /// <param name="payload">Payload exposed to Lua as a table.</param>
    void Publish(string eventName, IReadOnlyDictionary<string, object?> payload);

    /// <summary>
    /// Registers a Lua callback for a named event.
    /// </summary>
    /// <param name="eventName">Stable event name, such as <c>player.connected</c>.</param>
    /// <param name="callback">Lua closure to invoke when the event is published.</param>
    void Register(string eventName, Closure callback);
}
