namespace Moongate.Abstractions.Interfaces.Services;

/// <summary>Resolves named secrets without exposing their storage backend to consumers.</summary>
public interface ISecretManagerService
{
    /// <summary>Returns a secret by logical name, or null when the configured backend has no value.</summary>
    ValueTask<string?> GetSecretAsync(string name, CancellationToken cancellationToken = default);
}
