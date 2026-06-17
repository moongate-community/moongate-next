using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Server.Handlers.World;

/// <summary>Broadcasts a completed mobile step to nearby players via the interest service.</summary>
public sealed class MovementBroadcastHandler : IAsyncEventHandler<MobileMovedEvent>
{
    private readonly IInterestManagementService _interest;

    public MovementBroadcastHandler(IInterestManagementService interest)
    {
        ArgumentNullException.ThrowIfNull(interest);

        _interest = interest;
    }

    public Task HandleAsync(MobileMovedEvent evt, CancellationToken cancellationToken)
        => _interest.OnMobileMovedAsync(evt, cancellationToken);
}
