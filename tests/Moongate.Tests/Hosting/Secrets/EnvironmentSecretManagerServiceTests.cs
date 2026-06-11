using Moongate.Abstractions.Data.Secrets;
using Moongate.Abstractions.Services.Secrets;
using Moongate.Abstractions.Types.Secrets;

namespace Moongate.Tests.Hosting.Secrets;

public sealed class EnvironmentSecretManagerServiceTests
{
    [Fact]
    public async Task GetSecretAsync_BlankName_ReturnsNull()
    {
        var service = new EnvironmentSecretManagerService(new EnvironmentSecretManagerConfig());

        var secret = await service.GetSecretAsync("   ");

        Assert.Null(secret);
    }

    [Fact]
    public async Task GetSecretAsync_MapsLogicalNameToPrefixedEnvironmentVariable()
    {
        var service = new EnvironmentSecretManagerService(
            new EnvironmentSecretManagerConfig
            {
                Prefix = "MOONGATE_EMAIL_"
            },
            static name => name == "MOONGATE_EMAIL_SMTP_PASSWORD" ? "secret-value" : null
        );

        var secret = await service.GetSecretAsync("smtp_password");

        Assert.Equal("secret-value", secret);
    }

    [Fact]
    public async Task GetSecretAsync_MissingVariable_ReturnsNull()
    {
        var service = new EnvironmentSecretManagerService(
            new EnvironmentSecretManagerConfig
            {
                Prefix = "MOONGATE_EMAIL_"
            },
            static _ => null
        );

        var secret = await service.GetSecretAsync("smtp_password");

        Assert.Null(secret);
    }

    [Theory]
    [InlineData("smtp-password", "MOONGATE_EMAIL_SMTP_PASSWORD")]
    [InlineData(" smtp password ", "MOONGATE_EMAIL_SMTP_PASSWORD")]
    [InlineData("smtp.password", "MOONGATE_EMAIL_SMTP_PASSWORD")]
    public void ResolveEnvironmentVariableName_NormalizesLogicalName(string secretName, string expected)
    {
        var service = new EnvironmentSecretManagerService(
            new EnvironmentSecretManagerConfig
            {
                Prefix = "MOONGATE_EMAIL_"
            },
            static _ => null
        );

        var variableName = service.ResolveEnvironmentVariableName(secretName);

        Assert.Equal(expected, variableName);
    }

    [Fact]
    public void SecretManagerConfig_InvalidProvider_ReturnsValidationError()
    {
        var config = new SecretManagerConfig
        {
            Provider = (SecretManagerProviderType)99
        };

        var error = Assert.Single(config.Validate());
        Assert.Contains("not supported", error);
    }
}
