namespace Moongate.Abstractions.Data.Seed;

/// <summary>
/// Boot-time seed action executed after the server publishes <c>ServerStartedEvent</c>.
/// </summary>
public delegate ValueTask SeedAction(IServiceProvider serviceProvider, CancellationToken cancellationToken);
