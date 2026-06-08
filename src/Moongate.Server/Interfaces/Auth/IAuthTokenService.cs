using Moongate.Server.Data.Auth;

namespace Moongate.Server.Interfaces.Auth;

/// <summary>
/// Issues and revokes access and refresh tokens for web clients.
/// </summary>
public interface IAuthTokenService
{
    /// <summary>
    /// Validates credentials and creates a new token pair.
    /// </summary>
    /// <param name="username">Account username.</param>
    /// <param name="password">Plain account password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token pair when credentials are valid; otherwise null.</returns>
    ValueTask<AuthTokenResponse?> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Exchanges an active refresh token for a new access token and refresh token.
    /// </summary>
    /// <param name="refreshToken">Opaque refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New token pair when the refresh token is active; otherwise null.</returns>
    ValueTask<AuthTokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes an active refresh token.
    /// </summary>
    /// <param name="refreshToken">Opaque refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when a token was revoked; otherwise false.</returns>
    ValueTask<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
