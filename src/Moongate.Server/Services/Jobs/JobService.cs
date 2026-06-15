using System.Diagnostics;
using Moongate.Abstractions.Data.Jobs;
using Moongate.Abstractions.Interfaces.Jobs;
using Moongate.Abstractions.Interfaces.Timing;
using Moongate.Abstractions.Types.Jobs;
using Moongate.Server.Services.Jobs.Internal;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Jobs;

/// <summary>
/// Registry of named operational jobs layered over <see cref="ITimerService" />. Each job registers an
/// underlying timer whose callback is wrapped to record run metadata; jobs run on the game-loop thread.
/// </summary>
public sealed class JobService : IJobService
{
    private readonly ILogger _logger = Log.ForContext<JobService>();
    private readonly ITimerService _timers;
    private readonly Dictionary<string, JobEntry> _jobs = new(StringComparer.Ordinal);
    private readonly Lock _sync = new();

    public JobService(ITimerService timers)
    {
        ArgumentNullException.ThrowIfNull(timers);
        _timers = timers;
    }

    public string RegisterRecurring(
        string name,
        TimeSpan interval,
        Action handler,
        string? description = null,
        bool runImmediately = false,
        JobSourceType source = JobSourceType.CSharp
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(handler);

        var entry = NewEntry(name, description, source, interval, repeat: true, handler);
        var delay = runImmediately ? TimeSpan.FromMilliseconds(1) : interval;

        lock (_sync)
        {
            _jobs[entry.Id] = entry;
            entry.NextRunAt = DateTimeOffset.UtcNow + delay;
            entry.TimerId = _timers.RegisterTimer(name, interval, () => Execute(entry), delay, repeat: true);
        }

        return entry.Id;
    }

    public string RegisterOnce(
        string name,
        TimeSpan delay,
        Action handler,
        string? description = null,
        JobSourceType source = JobSourceType.CSharp
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(handler);

        var entry = NewEntry(name, description, source, delay, repeat: false, handler);

        lock (_sync)
        {
            _jobs[entry.Id] = entry;
            entry.NextRunAt = DateTimeOffset.UtcNow + delay;
            entry.TimerId = _timers.RegisterTimer(name, delay, () => Execute(entry), delay, repeat: false);
        }

        return entry.Id;
    }

    public bool RunNow(string jobId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId);

        JobEntry? entry;

        lock (_sync)
        {
            if (!_jobs.TryGetValue(jobId, out entry))
            {
                return false;
            }
        }

        _timers.RegisterTimer(
            entry.Name,
            entry.Interval,
            () => Execute(entry),
            TimeSpan.FromMilliseconds(1),
            repeat: false
        );

        return true;
    }

    public bool Cancel(string jobId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId);

        lock (_sync)
        {
            if (!_jobs.Remove(jobId, out var entry))
            {
                return false;
            }

            if (entry.TimerId is not null)
            {
                _timers.UnregisterTimer(entry.TimerId);
            }

            return true;
        }
    }

    public IReadOnlyList<JobSnapshot> GetJobs()
    {
        lock (_sync)
        {
            return _jobs.Values
                        .Select(
                            entry => new JobSnapshot(
                                entry.Id,
                                entry.Name,
                                entry.Description,
                                entry.Source,
                                entry.Interval.TotalMilliseconds,
                                entry.Repeat,
                                entry.NextRunAt,
                                entry.LastRunAt,
                                entry.LastDurationMs,
                                entry.LastStatus,
                                entry.LastError,
                                entry.RunCount
                            )
                        )
                        .OrderBy(job => job.Name, StringComparer.OrdinalIgnoreCase)
                        .ToArray();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    private static JobEntry NewEntry(
        string name,
        string? description,
        JobSourceType source,
        TimeSpan interval,
        bool repeat,
        Action handler
    )
        => new()
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            Description = description,
            Source = source,
            Interval = interval,
            Repeat = repeat,
            Handler = handler
        };

    private void Execute(JobEntry entry)
    {
        var stopwatch = Stopwatch.StartNew();
        JobStatusType status;
        string? error = null;

        try
        {
            entry.Handler();
            status = JobStatusType.Success;
        }
        catch (Exception caught)
        {
            status = JobStatusType.Failed;
            error = caught.Message;
            _logger.Error(caught, "Job {JobName} ({JobId}) failed", entry.Name, entry.Id);
        }

        stopwatch.Stop();

        lock (_sync)
        {
            entry.RunCount++;
            entry.LastRunAt = DateTimeOffset.UtcNow;
            entry.LastDurationMs = stopwatch.Elapsed.TotalMilliseconds;
            entry.LastStatus = status;
            entry.LastError = error;
            entry.NextRunAt = entry.Repeat ? entry.LastRunAt + entry.Interval : null;
        }
    }
}
