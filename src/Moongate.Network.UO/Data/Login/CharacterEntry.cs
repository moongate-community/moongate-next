namespace Moongate.Network.UO.Data.Login;

public sealed class CharacterEntry
{
    public CharacterEntry(string name = "", string password = "")
    {
        Name = name;
        Password = password;
    }

    public static int Length => 60;

    public string Name { get; set; }

    public string Password { get; set; }
}
