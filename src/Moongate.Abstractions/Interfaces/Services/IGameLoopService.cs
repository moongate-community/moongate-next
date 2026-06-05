namespace Moongate.Abstractions.Interfaces.Services;

/// <summary>
/// Owns the dedicated game-loop thread that drains tick events from <see cref="IEventBusService" />.
/// </summary>
public interface IGameLoopService : IMoongateService
{
    /// <summary>Number of loop iterations performed since start.</summary>
    long TickCount { get; }

    /// <summary>Exponential moving average of per-tick elapsed time in milliseconds.</summary>
    double AverageTickMs { get; }

    /// <summary>Worst observed tick elapsed time in milliseconds.</summary>
    double MaxTickMs { get; }
}
