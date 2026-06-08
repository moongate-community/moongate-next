using DryIoc;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Core.Ids;
using Moongate.Persistence.Extensions.DryIoc;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Server.Data.Auth;
using Moongate.Server.Data.Config;
using Moongate.Server.Interfaces.Auth;
using Moongate.Server.Services.Auth;
using Moongate.UO.Domain.Interfaces.Services;

namespace Moongate.Server.Extensions.Auth;

public static class AuthContainerExtensions
{
    private const ushort AuthRefreshTokenEntityTypeId = 2;
    private const int AuthRefreshTokenEntitySchemaVersion = 1;

    public static IContainer AddMoongateAuth(this IContainer container)
    {
        container.RegisterConfigSection("web", () => new WebConfig());
        container.RegisterPersistenceEntity<AuthRefreshTokenEntity, Serial>(
            AuthRefreshTokenEntityTypeId,
            AuthRefreshTokenEntitySchemaVersion,
            token => token.Id
        );
        container.RegisterDelegate<IAuthTokenService>(
            resolver => new AuthTokenService(
                () => resolver.Resolve<IUserService>(),
                () => resolver.Resolve<IAutoDataAccess<AuthRefreshTokenEntity, Serial>>(),
                resolver.Resolve<WebConfig>()
            ),
            Reuse.Singleton
        );

        return container;
    }
}
