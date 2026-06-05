namespace Moongate.Abstractions.Interfaces.Config;

/// <summary>
/// Optional contract for config sections that validate their own values at load time.
/// </summary>
public interface IValidatableConfig
{
    /// <summary>Returns a validation error message per invalid value; empty when valid.</summary>
    IEnumerable<string> Validate();
}
