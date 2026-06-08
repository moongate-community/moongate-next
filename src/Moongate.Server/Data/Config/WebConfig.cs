using Moongate.Abstractions.Interfaces.Config;

namespace Moongate.Server.Data.Config;

public sealed class WebConfig : IValidatableConfig
{
    public JwtConfig Jwt { get; set; } = new();

    public IEnumerable<string> Validate()
        => Jwt.Validate();
}
