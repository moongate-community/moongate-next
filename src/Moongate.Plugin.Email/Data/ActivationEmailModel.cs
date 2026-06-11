namespace Moongate.Plugin.Email.Data;

/// <summary>Allow-listed model exposed to Liquid activation email templates.</summary>
public sealed record ActivationEmailModel(
    string Username,
    string Email,
    string ActivationId,
    string ActivationUrl
);
