namespace Moongate.Core.Types;

/// <summary>
/// Access/permission level of an account, shared across authentication, user
/// management and entity visibility.
/// </summary>
public enum UserLevelType
{
    /// <summary>Regular player account.</summary>
    Player = 0,

    /// <summary>Game master account.</summary>
    GameMaster = 1,

    /// <summary>Administrator account.</summary>
    Administrator = 2
}
