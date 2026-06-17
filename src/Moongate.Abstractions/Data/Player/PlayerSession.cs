using Moongate.Abstractions.Data.Version;
using Moongate.Abstractions.Types.Player;
using Moongate.Core.Ids;

namespace Moongate.Abstractions.Data.Player;

/// <summary>
/// Logical player session state associated with a network session.
/// </summary>
public sealed class PlayerSession
{
    public long SessionId { get; set; }
    public string? RemoteEndPoint { get; set; }
    public Serial? UserId { get; set; }
    public string? Username { get; set; }
    public Serial? CharacterSerial { get; set; }
    public Serial? MobileSerial { get; set; }
    public PlayerSessionStateType State { get; set; } = PlayerSessionStateType.Connected;
    public DateTimeOffset ConnectedAt { get; set; }
    public DateTimeOffset? AuthenticatedAt { get; set; }
    public DateTimeOffset? EnteredWorldAt { get; set; }
    public DateTimeOffset? DisconnectedAt { get; set; }
    public int? ViewRange { get; set; }
    public ClientVersion? ClientVersion { get; set; }

    public byte PingSequence { get; set; }

    public byte MoveSequence { get; set; }
    public long MoveCredit { get; set; }
    public long MoveTime { get; set; }
}
