using System.Net;
using Moongate.Abstractions.Interfaces.Network;

namespace Moongate.Tests.Support;

/// <summary>
/// Minimal <see cref="IGameSession" /> stand-in for constructing packet contexts in tests.
/// </summary>
public sealed class FakeGameSession : IGameSession
{
    public long SessionId { get; init; }

    public IPEndPoint? ClientEndPoint { get; init; }

    public IPEndPoint? ServerEndPoint { get; init; }

    public uint? Seed { get; init; }

    public int CompressionEnabledCalls { get; private set; }

    public void EnableCompression()
        => CompressionEnabledCalls++;
}
