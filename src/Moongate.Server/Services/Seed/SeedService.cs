using Moongate.Abstractions.Data.Seed;
using Moongate.Server.Interfaces.Seed;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Seed;

/// <summary>
///     Executes registered boot-time seed actions.
/// </summary>
public sealed class SeedService : ISeedService
{
    private readonly IReadOnlyList<SeedAction> _actions;
    private readonly ILogger _logger = Log.ForContext<SeedService>();
    private readonly IServiceProvider _serviceProvider;
    private int _hasRun;

    public SeedService(IServiceProvider serviceProvider, IReadOnlyList<SeedAction> actions)
    {
        _serviceProvider = serviceProvider;
        _actions = actions;
    }

    public async ValueTask RunAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _hasRun, 1) == 1)
        {
            return;
        }

        _logger.Information("Running {SeedActionCount} seed action(s)", _actions.Count);

        for (var i = 0; i < _actions.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _actions[i](_serviceProvider, cancellationToken);
        }

        _logger.Information("Seed actions completed");
    }
}
