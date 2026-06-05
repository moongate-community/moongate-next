using Moongate.Abstractions.Interfaces.Config;

namespace Moongate.Tests.Hosting.Configuration.Support;

public sealed class ValidatableSettings : IValidatableConfig
{
    public int MaxPlayers { get; set; } = 100;

    public IEnumerable<string> Validate()
    {
        if (MaxPlayers <= 0)
        {
            yield return "MaxPlayers must be greater than 0.";
        }
    }
}
