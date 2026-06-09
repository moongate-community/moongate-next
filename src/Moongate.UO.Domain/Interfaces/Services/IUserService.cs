using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.UO.Domain.Entities;

namespace Moongate.UO.Domain.Interfaces.Services;

/// <summary>
/// Provides account-level access to UO users for the server and plugins.
/// </summary>
public interface IUserService : IPaginatedService<UserEntity>
{
    /// <summary>Returns the current number of persisted users.</summary>
    ValueTask<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a user with an auto-allocated serial and a hashed password.</summary>
    ValueTask<UserEntity> CreateAsync(
        string username,
        string email,
        string password,
        UserLevelType level = UserLevelType.Player,
        bool isActive = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>Permanently removes a user; false when absent.</summary>
    ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default);

    /// <summary>Gets a user by serial, or null when absent.</summary>
    ValueTask<UserEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default);

    /// <summary>Gets a user by username using case-insensitive matching, or null when absent.</summary>
    ValueTask<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>Replaces a user's password with a freshly hashed value; false when absent.</summary>
    ValueTask<bool> ResetPasswordAsync(Serial id, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>Locks or unlocks a user by toggling IsActive; returns null when absent.</summary>
    ValueTask<UserEntity?> SetActiveAsync(Serial id, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>Updates a user's email and level; returns null when the user is absent.</summary>
    ValueTask<UserEntity?> UpdateAsync(
        Serial id,
        string email,
        UserLevelType level,
        CancellationToken cancellationToken = default
    );
}
