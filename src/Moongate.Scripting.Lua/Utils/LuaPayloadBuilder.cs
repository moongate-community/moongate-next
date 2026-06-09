using Moongate.Abstractions.Data.Commands;
using Moongate.Abstractions.Data.Player;

namespace Moongate.Scripting.Lua.Utils;

/// <summary>
/// Builds stable snake_case payload dictionaries exposed to Lua callbacks.
/// </summary>
public static class LuaPayloadBuilder
{
    public static IReadOnlyDictionary<string, object?> Command(CommandSystemContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var payload = new Dictionary<string, object?>
        {
            ["command"] = context.CommandText,
            ["args"] = context.Arguments.Cast<object?>().ToArray(),
            ["source"] = context.Source.ToString(),
            ["session_id"] = context.SessionId,
            ["is_in_game"] = context.IsInGame
        };

        if (context.PlayerSession is { } playerSession)
        {
            payload["player"] = PlayerSession(playerSession);
        }

        return payload;
    }

    public static IReadOnlyDictionary<string, object?> PlayerConnection(
        long sessionId,
        string? remoteEndPoint,
        DateTimeOffset at
    )
        => new Dictionary<string, object?>
        {
            ["session_id"] = sessionId,
            ["remote_endpoint"] = remoteEndPoint,
            ["at"] = at
        };

    public static IReadOnlyDictionary<string, object?> PlayerSession(PlayerSession playerSession)
    {
        ArgumentNullException.ThrowIfNull(playerSession);

        return new Dictionary<string, object?>
        {
            ["session_id"] = playerSession.SessionId,
            ["username"] = playerSession.Username,
            ["user_id"] = playerSession.UserId?.ToString(),
            ["state"] = playerSession.State.ToString(),
            ["character_serial"] = playerSession.CharacterSerial?.ToString(),
            ["mobile_serial"] = playerSession.MobileSerial?.ToString()
        };
    }

    public static IReadOnlyDictionary<string, object?> ServerStarted(DateTimeOffset at)
        => new Dictionary<string, object?>
        {
            ["at"] = at
        };

    public static IReadOnlyDictionary<string, object?> Timer(string name, bool repeat)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Dictionary<string, object?>
        {
            ["name"] = name,
            ["repeat"] = repeat
        };
    }
}
