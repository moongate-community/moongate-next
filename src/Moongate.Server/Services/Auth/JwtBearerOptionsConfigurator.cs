using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moongate.Server.Data.Config;
using Moongate.Server.Hubs;
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
        options.TokenValidationParameters = new TokenValidationParameters
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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = ResolveWebSocketToken(
                    context.HttpContext.Request.Query,
                    context.HttpContext.Request.Path
                );

                if (token is not null)
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    }

    /// <summary>
    ///     Pulls the bearer token from the <c>access_token</c> query string for SignalR WebSocket
    ///     requests to the console hub (browsers can't set the Authorization header on a WS handshake).
    ///     Returns null for any other path or when the token is absent.
    /// </summary>
    internal static string? ResolveWebSocketToken(IQueryCollection query, PathString path)
    {
        string? token = query["access_token"];

        if (!string.IsNullOrEmpty(token) && path.StartsWithSegments(LiveConsoleHub.Route))
        {
            return token;
        }

        return null;
    }
}
