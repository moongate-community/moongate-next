using Moongate.Core.Ids;
using Moongate.UO.Domain.Types;

namespace Moongate.UO.Domain.Entities;

public sealed class UserEntity
{
    public Serial Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public UserLevelType Level { get; set; }
    public bool IsActive { get; set; }

    public UserEntity(Serial id, string username, string password, UserLevelType level, bool isActive)
    {
        Id = id;
        Username = username;
        Password = password;
        Level = level;
        IsActive = isActive;
    }
}
