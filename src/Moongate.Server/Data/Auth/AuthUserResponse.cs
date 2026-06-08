namespace Moongate.Server.Data.Auth;

/// <summary>Represents the authenticated web user.</summary>
public sealed record AuthUserResponse(
    string Id,
    string Username,
    string Level,
    bool IsActive
);
