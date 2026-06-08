using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moongate.Server.Data.Config;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Auth;

public sealed class JwtBearerOptionsConfigurator : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly ILogger _logger = Log.ForContext<JwtBearerOptionsConfigurator>();
    private readonly WebConfig _webConfig;

    public JwtBearerOptionsConfigurator(WebConfig webConfig)
    {
        _webConfig = webConfig;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (!string.Equals(name, JwtBearerDefaults.AuthenticationScheme, StringComparison.Ordinal))
        {
            return;
        }

        Configure(options);
    }

    public void Configure(JwtBearerOptions options)
    {
        var jwt = _webConfig.Jwt;

        if (jwt.IsUsingDevelopmentSigningKey)
        {
            _logger.Warning("Using development JWT signing key. Configure web.jwt.signing_key for production.");
        }

        options.MapInboundClaims = true;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    }
}
