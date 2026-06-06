using Moongate.Abstractions.Interfaces.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.WorldData;

/// <summary>
/// Registers server asset world data services before network services start.
/// </summary>
public sealed class ServerAssetDataBootService : IMoongateService
{
    private readonly ILogger _logger = Log.ForContext<ServerAssetDataBootService>();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        _logger.Information("World data services registered; YAML data will load lazily on first access");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        return Task.CompletedTask;
    }
}
