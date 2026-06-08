using System.Text;
using YamlDotNet.Serialization;

namespace Moongate.Server.Data.Config;

public sealed class JwtConfig
{
    public const string DevelopmentSigningKey = "MOONGATE_DEVELOPMENT_ONLY_SIGNING_KEY_CHANGE_ME_2026";

    public string Issuer { get; set; } = "Moongate";
    public string Audience { get; set; } = "Moongate.Web";
    public string SigningKey { get; set; } = DevelopmentSigningKey;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 14;
    public bool RotateRefreshTokens { get; set; } = true;

    [YamlIgnore]
    public bool IsUsingDevelopmentSigningKey => string.Equals(SigningKey, DevelopmentSigningKey, StringComparison.Ordinal);

    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
        {
            yield return "Jwt.Issuer is required.";
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            yield return "Jwt.Audience is required.";
        }

        if (string.IsNullOrWhiteSpace(SigningKey))
        {
            yield return "Jwt.SigningKey is required.";
        }
        else if (Encoding.UTF8.GetByteCount(SigningKey) < 32)
        {
            yield return "Jwt.SigningKey must be at least 32 UTF-8 bytes.";
        }

        if (AccessTokenMinutes <= 0)
        {
            yield return "Jwt.AccessTokenMinutes must be greater than zero.";
        }

        if (RefreshTokenDays <= 0)
        {
            yield return "Jwt.RefreshTokenDays must be greater than zero.";
        }
    }
}
