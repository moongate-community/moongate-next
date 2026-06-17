using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Moongate.Abstractions.Interfaces.Services;
using Serilog;

namespace Moongate.Abstractions.Internal;

/// <summary>
///     The single <see cref="IHostedService" /> registered with the host. Dispatches
///     <see cref="IHostedService.StartAsync" /> and <see cref="IHostedService.StopAsync" />
///     to every registered <see cref="IMoongateService" /> in priority order.
/// </summary>
internal sealed class MoongateServiceOrchestrator : IHostedService
{
    private readonly ILogger _logger = Log.ForContext<MoongateServiceOrchestrator>();
    private readonly MoongateServiceDescriptor[] _startOrder;
    private readonly MoongateServiceDescriptor[] _stopOrder;

    public MoongateServiceOrchestrator(IEnumerable<MoongateServiceDescriptor> descriptors)
    {
        // OrderBy is stable, so equal priorities preserve registration order.
        _startOrder = descriptors.OrderBy(d => d.Priority).ToArray();
        _stopOrder = _startOrder.Reverse().ToArray();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();

        for (var i = 0; i < _startOrder.Length; i++)
        {
            var descriptor = _startOrder[i];
            var serviceStartTime = Stopwatch.GetTimestamp();

            _logger.Debug(
                "Starting {Service} (priority {Priority})",
                descriptor.Service.GetType().Name,
                descriptor.Priority
            );

            await descriptor.Service.StartAsync(cancellationToken);

            var serviceElapsed = Stopwatch.GetElapsedTime(serviceStartTime);
            _logger.Debug(
                "Started {Service} in {ElapsedMilliseconds:F0} ms (priority {Priority})",
                descriptor.Service.GetType().Name,
                serviceElapsed.TotalMilliseconds,
                descriptor.Priority
            );
        }

        var elapsed = Stopwatch.GetElapsedTime(startTime);
        _logger.Information(
            "Moongate services started in {Elapsed} ({ElapsedMilliseconds:F0} ms)",
            elapsed,
            elapsed.TotalMilliseconds
        );
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < _stopOrder.Length; i++)
        {
            var descriptor = _stopOrder[i];
            _logger.Debug(
                "Stopping {Service} (priority {Priority})",
                descriptor.Service.GetType().Name,
                descriptor.Priority
            );

            try
            {
                await descriptor.Service.StopAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Continue stopping the rest even if one service throws.
                _logger.Error(ex, "Error stopping {Service}", descriptor.Service.GetType().Name);
            }
        }
    }
}
