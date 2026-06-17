using System.Diagnostics;
using Moongate.Abstractions.Data;
using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Interfaces.Timing;
using Moongate.Abstractions.Types.Metrics;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.GameLoop;

/// <summary>
///     Owns the dedicated game-loop thread. Drains tick events from <see cref="IEventBusService" />,
///     sleeps when idle, exposes basic metrics.
/// </summary>
public sealed class GameLoopService : IGameLoopService, IMetricProvider, IDisposable
{
    private const int MaxTickEventsPerFrame = 256;
    private const double SlowTickThresholdMs = 250;
    private readonly IEventBusService _bus;
    private readonly GameLoopConfig _config;
    private readonly CancellationTokenSource _cts = new();

    private readonly ILogger _logger = Log.ForContext<GameLoopService>();
    private readonly Lock _metricsSync = new();
    private readonly ITimerService? _timers;
    private double _averageTickMs;
    private long _idleSleepCount;
    private double _maxTickMs;

    private Thread? _thread;
    private long _tickCount;

    public GameLoopService(IEventBusService bus, GameLoopConfig config, ITimerService? timers = null)
    {
        _bus = bus;
        _config = config;
        _timers = timers;
    }

    public void Dispose()
    {
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }

    public long TickCount => Interlocked.Read(ref _tickCount);

    public double AverageTickMs
    {
        get
        {
            lock (_metricsSync)
            {
                return _averageTickMs;
            }
        }
    }

    public double MaxTickMs
    {
        get
        {
            lock (_metricsSync)
            {
                return _maxTickMs;
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _thread = new Thread(RunLoop)
        {
            IsBackground = true,
            Name = "Moongate-GameLoop"
        };
        _thread.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        _thread?.Join(TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    public string Prefix => "gameloop";

    public IReadOnlyList<MetricSample> Collect()
    {
        double avg,
            max;

        lock (_metricsSync)
        {
            avg = _averageTickMs;
            max = _maxTickMs;
        }

        return
        [
            new MetricSample(
                "tick_count",
                Interlocked.Read(ref _tickCount),
                MetricType.Counter,
                Help: "Total game loop iterations"
            ),
            new MetricSample(
                "tick_avg_ms",
                avg,
                Help: "EMA tick elapsed in ms"
            ),
            new MetricSample(
                "tick_max_ms",
                max,
                Help: "Worst tick elapsed in ms"
            ),
            new MetricSample(
                "idle_sleeps_total",
                Interlocked.Read(ref _idleSleepCount),
                MetricType.Counter,
                Help: "Total idle sleeps"
            )
        ];
    }

    private void RunLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            var tickStart = Stopwatch.GetTimestamp();
            var workUnits = _bus.DrainTickEvents(MaxTickEventsPerFrame);

            if (_timers is not null)
            {
                var nowMs = (long)Math.Floor(Stopwatch.GetTimestamp() * 1000.0 / Stopwatch.Frequency);
                workUnits += _timers.UpdateTicksDelta(nowMs);
            }

            var elapsed = Stopwatch.GetElapsedTime(tickStart);

            UpdateMetrics(elapsed);

            if (elapsed.TotalMilliseconds >= SlowTickThresholdMs)
            {
                _logger.Warning(
                    "Slow tick: {Elapsed:0.###}ms workUnits={WorkUnits} queueDepth={Queue}",
                    elapsed.TotalMilliseconds,
                    workUnits,
                    _bus.CurrentTickQueueDepth
                );
            }

            if (_config.IdleCpuEnabled && workUnits == 0)
            {
                Thread.Sleep(_config.IdleSleepMs);
                Interlocked.Increment(ref _idleSleepCount);
            }
        }
    }

    private void UpdateMetrics(TimeSpan elapsed)
    {
        Interlocked.Increment(ref _tickCount);

        lock (_metricsSync)
        {
            // Exponential moving average: 0.95 weight to history, 0.05 to current sample.
            _averageTickMs = _averageTickMs * 0.95 + elapsed.TotalMilliseconds * 0.05;
            _maxTickMs = Math.Max(_maxTickMs, elapsed.TotalMilliseconds);
        }
    }
}
