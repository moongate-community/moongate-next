namespace Moongate.Server.Services.Network.Internal;

/// <summary>
/// Per-session parse state for the initial UO seed phase. A connection begins with either the
/// <c>0xEF</c> login-seed packet (login server) or a raw 4-byte seed (game-server reconnect); the
/// parser consumes that before normal packet framing.
/// </summary>
internal sealed class PacketStreamState
{
    /// <summary>True once the initial seed (0xEF packet or raw 4 bytes) has been handled.</summary>
    public bool SeedConsumed { get; set; }

    /// <summary>The raw 4-byte seed captured on the game-server reconnect path; null otherwise.</summary>
    public uint? Seed { get; set; }
}
