using Moongate.Core.Ids;
using Moongate.Core.Types;

namespace Moongate.UO.Domain.Entities;

public sealed class UserEntity
{
    public Serial Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public UserLevelType Level { get; set; }
    public bool IsActive { get; set; }

    public UserEntity(Serial id, string username, string email, string password, UserLevelType level, bool isActive)
    {
        Id = id;
        Username = username;
        Email = email;
        Password = password;
        Level = level;
        IsActive = isActive;
    }
}
