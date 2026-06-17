namespace Moongate.Abstractions.Data;

/// <summary>
///     Configuration for the game loop thread.
/// </summary>
public sealed class GameLoopConfig
{
    /// <summary>
    ///     When <c>true</c>, the loop sleeps <see cref="IdleSleepMs" /> milliseconds whenever a tick
    ///     processes zero work units. When <c>false</c>, the loop spins continuously.
    /// </summary>
    public bool IdleCpuEnabled { get; set; } = true;

    /// <summary>
    ///     Milliseconds to sleep when a tick finds no work. Default 1 ms.
    /// </summary>
    public int IdleSleepMs { get; set; } = 1;
}
