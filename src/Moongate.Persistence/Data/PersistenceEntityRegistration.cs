using Moongate.Persistence.Interfaces.Persistence;

namespace Moongate.Persistence.Data;

/// <summary>
///     Boot-time registration item accumulated in the container and turned into a registry entry by the
///     persistence service at startup.
/// </summary>
public sealed class PersistenceEntityRegistration
{
    public PersistenceEntityRegistration(IPersistenceEntityDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        Descriptor = descriptor;
    }

    /// <summary>The descriptor to register at boot.</summary>
    public IPersistenceEntityDescriptor Descriptor { get; }
}
