namespace Moongate.Server.Data.Auth;

/// <summary>Payload for activating a pending user account.</summary>
public sealed record AuthActivationRequest
{
    /// <summary>Opaque activation id generated during registration.</summary>
    public string ActivationId { get; init; } = "";
}
