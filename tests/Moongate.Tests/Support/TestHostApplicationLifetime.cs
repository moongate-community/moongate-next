using Microsoft.Extensions.Hosting;

namespace Moongate.Tests.Support;

internal sealed class TestHostApplicationLifetime : IHostApplicationLifetime, IDisposable
{
    private readonly CancellationTokenSource _started = new();
    private readonly CancellationTokenSource _stopped = new();
    private readonly CancellationTokenSource _stopping = new();

    public void Dispose()
    {
        _started.Dispose();
        _stopping.Dispose();
        _stopped.Dispose();
    }

    public CancellationToken ApplicationStarted => _started.Token;

    public CancellationToken ApplicationStopping => _stopping.Token;

    public CancellationToken ApplicationStopped => _stopped.Token;

    public void StopApplication()
    {
        _stopping.Cancel();
    }

    public void Start()
    {
        _started.Cancel();
    }

    public void Stop()
    {
        _stopped.Cancel();
    }
}
