using Moongate.Abstractions.Interfaces.Services;

namespace Moongate.Abstractions.Internal;

/// <summary>
/// Pairs a registered <see cref="IMoongateService" /> with its start priority.
/// Lower priorities start first; stop happens in reverse order.
/// </summary>
internal sealed record MoongateServiceDescriptor
{
    public IMoongateService Service { get; }
    public int Priority { get; }

    public MoongateServiceDescriptor(IMoongateService service, int priority)
    {
        ArgumentNullException.ThrowIfNull(service);

        Service = service;
        Priority = priority;
    }
}
