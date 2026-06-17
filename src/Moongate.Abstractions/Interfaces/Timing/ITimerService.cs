using Moongate.Abstractions.Interfaces.Services;

namespace Moongate.Abstractions.Interfaces.Timing;

/// <summary>
///     Hashed timer wheel driven by the game loop. Callbacks run synchronously
///     on the game-loop thread, in publish order, with the same determinism as tick events.
/// </summary>
public interface ITimerService : IMoongateService
{
    /// <summary>
    ///     Registers a timer.
    /// </summary>
    /// <param name="name">Logical name (indexable for bulk cancel, e.g. "spell-tick-player42").</param>
    /// <param name="interval">Interval used as due-time for one-shot or period for repeating timers.</param>
    /// <param name="callback">Synchronous callback executed on the game-loop thread.</param>
    /// <param name="delay">Initial delay before first execution; if null, uses <paramref name="interval" />.</param>
    /// <param name="repeat">When true, callback re-fires every <paramref name="interval" />.</param>
    /// <returns>Opaque timer id used for cancellation.</returns>
    string RegisterTimer(
        string name,
        TimeSpan interval,
        Action callback,
        TimeSpan? delay = null,
        bool repeat = false
    );

    /// <summary>Cancels every registered timer.</summary>
    void UnregisterAllTimers();

    /// <summary>Cancels one timer by id. Returns true if a registered timer was found and removed.</summary>
    bool UnregisterTimer(string timerId);

    /// <summary>Cancels every timer with the given name. Returns the number of timers removed.</summary>
    int UnregisterTimersByName(string name);

    /// <summary>
    ///     Advances the wheel using an absolute monotonic timestamp in milliseconds.
    ///     Called by the game loop each iteration; do not call from application code.
    /// </summary>
    /// <returns>Number of wheel ticks processed.</returns>
    int UpdateTicksDelta(long timestampMilliseconds);
}
