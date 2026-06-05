using Microsoft.Extensions.Hosting;

namespace Moongate.Abstractions.Interfaces.Services;

/// <summary>
/// Marker interface for services orchestrated by Moongate's hosting layer.
/// </summary>
/// <remarks>
/// Extends <see cref="IHostedService" /> so existing .NET hosting conventions
/// (cancellation, async lifecycle, BackgroundService) keep working. The
/// per-service start priority is supplied at registration time via
/// <c>AddMoongateService</c>, not on the type itself.
/// </remarks>
public interface IMoongateService : IHostedService { }
