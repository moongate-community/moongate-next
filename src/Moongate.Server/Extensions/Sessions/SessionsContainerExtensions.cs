using DryIoc;
using Moongate.Server.Interfaces.Sessions;
using Moongate.Server.Services.Sessions;

namespace Moongate.Server.Extensions.Sessions;

/// <summary>
///     DryIoc-native registration for session-handoff services.
/// </summary>
public static class SessionsContainerExtensions
{
    /// <summary>Registers the game-login handoff service (login-to-game-server redirect bridge).</summary>
    public static IContainer AddMoongateGameLoginHandoff(this IContainer container)
    {
        container.Register<GameLoginHandoffService>(Reuse.Singleton);
        container.RegisterMapping<IGameLoginHandoffService, GameLoginHandoffService>();

        return container;
    }
}
