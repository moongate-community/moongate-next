using Moongate.Abstractions.Data.Internal;
using Moongate.Server.Data.Config;
using ConfigService = Moongate.Abstractions.Configuration.ConfigService;

namespace Moongate.Tests.Server.Auth;

public sealed class WebConfigTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"moongate-web-config-{Guid.NewGuid():N}");
    private string ConfigPath => Path.Combine(_dir, "moongate.yaml");

    [Fact]
    public void Defaults_AreValidAndUseDevelopmentSigningKey()
    {
        var config = new WebConfig();

        Assert.Empty(config.Validate());
        Assert.Equal("", config.BaseUrl);
        Assert.Equal("Moongate", config.Jwt.Issuer);
        Assert.Equal("Moongate.Web", config.Jwt.Audience);
        Assert.True(config.Jwt.IsUsingDevelopmentSigningKey);
    }

    [Fact]
    public void Load_WebSection_BindsBaseUrl()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(
            ConfigPath,
            """
            web:
              base_url: https://play.moongate.io
            """
        );

        var results = ConfigService.Load(ConfigPath, [WebSection()]);
        var config = Assert.IsType<WebConfig>(Assert.Single(results).Instance);

        Assert.Equal("https://play.moongate.io", config.BaseUrl);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Load_WebSection_BindsJwtValues()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(
            ConfigPath,
            """
            web:
              jwt:
                issuer: CustomIssuer
                audience: CustomAudience
                signing_key: custom-signing-key-with-more-than-32-chars
                access_token_minutes: 30
                refresh_token_days: 7
                rotate_refresh_tokens: false
            """
        );

        var results = ConfigService.Load(ConfigPath, [WebSection()]);
        var config = Assert.IsType<WebConfig>(Assert.Single(results).Instance);

        Assert.Equal("CustomIssuer", config.Jwt.Issuer);
        Assert.Equal("CustomAudience", config.Jwt.Audience);
        Assert.Equal("custom-signing-key-with-more-than-32-chars", config.Jwt.SigningKey);
        Assert.Equal(30, config.Jwt.AccessTokenMinutes);
        Assert.Equal(7, config.Jwt.RefreshTokenDays);
        Assert.False(config.Jwt.RotateRefreshTokens);
    }

    [Fact]
    public void Validate_ShortSigningKey_ReturnsError()
    {
        var config = new WebConfig
        {
            Jwt =
            {
                SigningKey = "short"
            }
        };

        var errors = config.Validate().ToArray();

        Assert.Contains(errors, error => error.Contains("SigningKey", StringComparison.Ordinal));
    }

    private static ConfigSectionRegistration WebSection()
        => new("web", typeof(WebConfig), () => new WebConfig());
}
