using Moongate.Abstractions.Data.Jobs;
using Moongate.Abstractions.Interfaces.Jobs;
using Moongate.Abstractions.Types.Jobs;

namespace Moongate.Tests.Support;

/// <summary>
///     IJobService stub that captures the recurring handler so tests can invoke it via <see cref="Invoke" />.
/// </summary>
public sealed class CapturingJobService : IJobService
{
    private Action? _handler;

    public bool Cancel(string jobId)
    {
        return true;
    }

    public IReadOnlyList<JobSnapshot> GetJobs()
    {
        return [];
    }

    public string RegisterOnce(
        string name,
        TimeSpan delay,
        Action handler,
        string? description = null,
        JobSourceType source = JobSourceType.CSharp
    )
    {
        throw new NotSupportedException();
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
        _handler = handler;

        return "capturing-job";
    }

    public bool RunNow(string jobId)
    {
        throw new NotSupportedException();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Invoke()
    {
        _handler?.Invoke();
    }
}
