using Moongate.Abstractions.Interfaces.Network;
using ZLinq;
using ZLinq.Linq;

namespace Moongate.Tests.Support;

/// <summary>
///     INetworkSessionManager no-op stub for tests that do not exercise session lookup.
///     All members throw <see cref="NotSupportedException" />.
/// </summary>
public sealed class NoopNetworkSessionManager : INetworkSessionManager
{
    public int Count => throw new NotSupportedException();

    public IReadOnlyCollection<long> GetSessionIds()
    {
        throw new NotSupportedException();
    }

    public ValueEnumerable<FromArray<long>, long> QuerySessionIds()
    {
        throw new NotSupportedException();
    }

    public bool TryGetSession(long sessionId, out IGameSession session)
    {
        throw new NotSupportedException();
    }
}
