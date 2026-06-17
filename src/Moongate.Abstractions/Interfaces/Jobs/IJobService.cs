using Moongate.Abstractions.Data.Jobs;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Types.Jobs;

namespace Moongate.Abstractions.Interfaces.Jobs;

/// <summary>
///     Registry of named operational jobs layered over <see cref="Timing.ITimerService" />. Jobs run on the
///     game-loop thread; the service records per-job run metadata for the admin UI.
/// </summary>
public interface IJobService : IMoongateService
{
    /// <summary>Cancels and removes a job. Returns false if unknown.</summary>
    bool Cancel(string jobId);

    /// <summary>Snapshots all registered jobs.</summary>
    IReadOnlyList<JobSnapshot> GetJobs();

    /// <summary>Registers a one-shot job that runs once after <paramref name="delay" />. Returns the job id.</summary>
    string RegisterOnce(
        string name,
        TimeSpan delay,
        Action handler,
        string? description = null,
        JobSourceType source = JobSourceType.CSharp
    );

    /// <summary>Registers a repeating job. Returns the job id.</summary>
    string RegisterRecurring(
        string name,
        TimeSpan interval,
        Action handler,
        string? description = null,
        bool runImmediately = false,
        JobSourceType source = JobSourceType.CSharp
    );

    /// <summary>Schedules an immediate extra run of a job (next game-loop frame). Returns false if unknown.</summary>
    bool RunNow(string jobId);
}
