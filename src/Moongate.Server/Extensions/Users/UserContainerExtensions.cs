using DryIoc;
using Moongate.Core.Ids;
using Moongate.Persistence.Extensions.DryIoc;
using Moongate.Server.Services.Users;
using Moongate.UO.Domain.Entities;
using Moongate.UO.Domain.Interfaces.Services;

namespace Moongate.Server.Extensions.Users;

/// <summary>
/// DryIoc-native registration helpers for UO user services.
/// </summary>
public static class UserContainerExtensions
{
    private const ushort UserEntityTypeId = 1;
    private const int UserEntitySchemaVersion = 1;

    /// <summary>
    /// Registers the UO user entity and user service available to server code and plugins.
    /// </summary>
    public static IContainer AddMoongateUsers(this IContainer container)
    {
        container.RegisterPersistenceEntity<UserEntity, Serial>(UserEntityTypeId, UserEntitySchemaVersion, user => user.Id);
        container.Register<IUserService, UserService>(Reuse.Singleton);

        return container;
    }
}
