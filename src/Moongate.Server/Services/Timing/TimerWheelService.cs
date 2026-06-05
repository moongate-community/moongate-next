using System.Diagnostics;
using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Data.Timing;
using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Abstractions.Interfaces.Timing;
using Moongate.Abstractions.Types.Metrics;
using Moongate.Server.Services.Timing.Internal;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Timing;

/// <summary>
/// Hashed timer wheel driven by <see cref="ITimerService.UpdateTicksDelta" /> from the game loop.
/// </summary>
public sealed class TimerWheelService : ITimerService, IMetricProvider
{
    private readonly ILogger _logger = Log.ForContext<TimerWheelService>();
    private readonly TimeSpan _tickDuration;
    private readonly double _tickDurationMs;
    private readonly LinkedList<TimerEntry>[] _wheel;
    private readonly Lock _syncRoot = new();
    private readonly Dictionary<string, TimerEntry> _timersById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HashSet<string>> _timerIdsByName = new(StringComparer.Ordinal);

    private long _currentTick;
    private long _lastTimestampMilliseconds = -1;
    private double _accumulatedMilliseconds;

    private long _totalRegistered;
    private long _totalExecuted;
    private long _callbackErrors;
    private long _totalCallbackElapsedStopwatchTicks;
    private long _totalProcessedTicks;

    public string Prefix => "timer";

    public TimerWheelService(TimerWheelConfig config)
    {
        if (config.TickDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(config),
                "TimerWheelConfig.TickDuration must be positive."
            );
        }

        if (config.WheelSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(config),
                "TimerWheelConfig.WheelSize must be positive."
            );
        }

        _tickDuration = config.TickDuration;
        _tickDurationMs = _tickDuration.TotalMilliseconds;
        _wheel = new LinkedList<TimerEntry>[config.WheelSize];

        for (var i = 0; i < _wheel.Length; i++)
        {
            _wheel[i] = new();
        }
    }

    public IReadOnlyList<MetricSample> Collect()
    {
        int active;

        lock (_syncRoot)
        {
            active = _timersById.Count;
        }

        var executed = Interlocked.Read(ref _totalExecuted);
        var elapsedSwTicks = Interlocked.Read(ref _totalCallbackElapsedStopwatchTicks);
        var avgMs = executed == 0
                        ? 0
                        : Stopwatch.GetElapsedTime(0, elapsedSwTicks / executed).TotalMilliseconds;

        return
        [
            new(
                "active",
                active,
                Help: "Currently registered timers"
            ),
            new(
                "registered_total",
                Interlocked.Read(ref _totalRegistered),
                MetricType.Counter,
                Help: "Total registrations since start"
            ),
            new(
                "executed_total",
                executed,
                MetricType.Counter,
                Help: "Total callback invocations"
            ),
            new(
                "callback_errors_total",
                Interlocked.Read(ref _callbackErrors),
                MetricType.Counter,
                Help: "Total callback exceptions"
            ),
            new(
                "callback_avg_ms",
                avgMs,
                Help: "Average callback duration"
            ),
            new(
                "processed_ticks_total",
                Interlocked.Read(ref _totalProcessedTicks),
                MetricType.Counter,
                Help: "Total wheel ticks processed"
            )
        ];
    }

    public string RegisterTimer(
        string name,
        TimeSpan interval,
        Action callback,
        TimeSpan? delay = null,
        bool repeat = false
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Timer name cannot be empty.", nameof(name));
        }

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be positive.");
        }

        ArgumentNullException.ThrowIfNull(callback);

        var dueTime = delay ?? interval;

        if (dueTime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), "Delay must be positive.");
        }

        var entry = new TimerEntry
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            Callback = callback,
            Interval = interval,
            Repeat = repeat
        };

        lock (_syncRoot)
        {
            _timersById[entry.Id] = entry;
            Interlocked.Increment(ref _totalRegistered);

            if (!_timerIdsByName.TryGetValue(name, out var ids))
            {
                ids = [];
                _timerIdsByName[name] = ids;
            }

            ids.Add(entry.Id);
            ScheduleEntry(entry, dueTime);
        }

        return entry.Id;
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        UnregisterAllTimers();

        return Task.CompletedTask;
    }

    public void UnregisterAllTimers()
    {
        lock (_syncRoot)
        {
            _timersById.Clear();
            _timerIdsByName.Clear();

            foreach (var bucket in _wheel)
            {
                bucket.Clear();
            }
        }
    }

    public bool UnregisterTimer(string timerId)
    {
        if (string.IsNullOrWhiteSpace(timerId))
        {
            return false;
        }

        lock (_syncRoot)
        {
            return RemoveEntryById(timerId);
        }
    }

    public int UnregisterTimersByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return 0;
        }

        lock (_syncRoot)
        {
            if (!_timerIdsByName.TryGetValue(name, out var ids) || ids.Count == 0)
            {
                return 0;
            }

            var timerIds = ids.ToArray();
            var removed = 0;

            foreach (var timerId in timerIds)
            {
                if (RemoveEntryById(timerId))
                {
                    removed++;
                }
            }

            return removed;
        }
    }

    public int UpdateTicksDelta(long timestampMilliseconds)
    {
        if (timestampMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timestampMilliseconds),
                "Timestamp must be non-negative."
            );
        }

        long ticksToProcess;

        lock (_syncRoot)
        {
            if (_lastTimestampMilliseconds < 0)
            {
                _lastTimestampMilliseconds = timestampMilliseconds;

                return 0;
            }

            var deltaMilliseconds = timestampMilliseconds - _lastTimestampMilliseconds;

            if (deltaMilliseconds <= 0)
            {
                return 0;
            }

            _lastTimestampMilliseconds = timestampMilliseconds;
            _accumulatedMilliseconds += deltaMilliseconds;
            ticksToProcess = (long)Math.Floor(_accumulatedMilliseconds / _tickDurationMs);

            if (ticksToProcess <= 0)
            {
                return 0;
            }

            _accumulatedMilliseconds -= ticksToProcess * _tickDurationMs;
        }

        for (var i = 0; i < ticksToProcess; i++)
        {
            ProcessTick();
        }

        Interlocked.Add(ref _totalProcessedTicks, ticksToProcess);

        return ticksToProcess > int.MaxValue ? int.MaxValue : (int)ticksToProcess;
    }

    private void ExecuteEntry(TimerEntry entry)
    {
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            entry.Callback();
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _callbackErrors);
            _logger.Error(
                ex,
                "Timer callback failed for timer '{TimerName}' ({TimerId})",
                entry.Name,
                entry.Id
            );
        }
        finally
        {
            Interlocked.Increment(ref _totalExecuted);
            Interlocked.Add(
                ref _totalCallbackElapsedStopwatchTicks,
                Stopwatch.GetTimestamp() - startedAt
            );
        }

        if (!entry.Repeat)
        {
            return;
        }

        lock (_syncRoot)
        {
            if (!entry.Cancelled && _timersById.ContainsKey(entry.Id))
            {
                ScheduleEntry(entry, entry.Interval);
            }
        }
    }

    private void ProcessTick()
    {
        List<TimerEntry> dueEntries = [];

        lock (_syncRoot)
        {
            _currentTick++;
            var slotIndex = (int)(_currentTick % _wheel.Length);
            var bucket = _wheel[slotIndex];
            var node = bucket.First;

            while (node is not null)
            {
                var next = node.Next;
                var entry = node.Value;

                if (entry.Cancelled)
                {
                    bucket.Remove(node);
                    entry.Node = null;
                    node = next;

                    continue;
                }

                if (entry.RemainingRounds > 0)
                {
                    entry.RemainingRounds--;
                    node = next;

                    continue;
                }

                bucket.Remove(node);
                entry.Node = null;
                dueEntries.Add(entry);

                if (!entry.Repeat)
                {
                    RemoveFromIndexes(entry);
                }

                node = next;
            }
        }

        foreach (var entry in dueEntries)
        {
            ExecuteEntry(entry);
        }
    }

    private bool RemoveEntryById(string timerId)
    {
        if (!_timersById.TryGetValue(timerId, out var entry))
        {
            return false;
        }

        entry.Cancelled = true;

        if (entry.Node is not null)
        {
            _wheel[entry.SlotIndex].Remove(entry.Node);
            entry.Node = null;
        }

        RemoveFromIndexes(entry);

        return true;
    }

    private void RemoveFromIndexes(TimerEntry entry)
    {
        _timersById.Remove(entry.Id);

        if (!_timerIdsByName.TryGetValue(entry.Name, out var ids))
        {
            return;
        }

        ids.Remove(entry.Id);

        if (ids.Count == 0)
        {
            _timerIdsByName.Remove(entry.Name);
        }
    }

    private void ScheduleEntry(TimerEntry entry, TimeSpan dueTime)
    {
        var ticks = ToWheelTicks(dueTime);
        var targetTick = _currentTick + ticks;
        var slotIndex = (int)(targetTick % _wheel.Length);
        var rounds = (ticks - 1) / _wheel.Length;

        entry.SlotIndex = slotIndex;
        entry.RemainingRounds = rounds;
        entry.Cancelled = false;
        entry.Node = _wheel[slotIndex].AddLast(entry);
    }

    private long ToWheelTicks(TimeSpan dueTime)
    {
        var ticks = (long)Math.Ceiling(dueTime.TotalMilliseconds / _tickDurationMs);

        return Math.Max(1, ticks);
    }
}
