namespace Moongate.Server.Data.Users;

/// <summary>Payload to lock or unlock a user.</summary>
public sealed record SetUserActiveRequest
{
    public bool IsActive { get; init; }
}
