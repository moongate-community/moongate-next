using Moongate.Abstractions.Data.Commands;
using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Types.Commands;
using Moongate.Abstractions.Types.Player;
using Moongate.Core.Ids;
using Moongate.Scripting.Lua.Utils;

namespace Moongate.Tests.Scripting.Lua;

public sealed class LuaPayloadBuilderTests
{
    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType)
            => null;
    }

    [Fact]
    public void Command_WithPlayerSession_BuildsStableLuaPayload()
    {
        var playerSession = new PlayerSession
        {
            SessionId = 42,
            UserId = (Serial)0x00000001u,
            Username = "admin",
            State = PlayerSessionStateType.InWorld,
            CharacterSerial = (Serial)0x00000010u,
            MobileSerial = (Serial)0x00000020u
        };
        var context = new CommandSystemContext(
            "lua_echo hello",
            ["hello"],
            CommandSourceType.InGame,
            new EmptyServiceProvider(),
            static (_, _) => { },
            42,
            playerSession
        );

        var payload = LuaPayloadBuilder.Command(context);
        var player = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(payload["player"]);

        Assert.Equal("lua_echo hello", payload["command"]);
        Assert.Equal(["hello"], Assert.IsType<object?[]>(payload["args"]));
        Assert.Equal("InGame", payload["source"]);
        Assert.Equal(42L, payload["session_id"]);
        Assert.True((bool)payload["is_in_game"]!);
        Assert.Equal(42L, player["session_id"]);
        Assert.Equal("admin", player["username"]);
        Assert.Equal("0x00000001", player["user_id"]);
        Assert.Equal("InWorld", player["state"]);
        Assert.Equal("0x00000010", player["character_serial"]);
        Assert.Equal("0x00000020", player["mobile_serial"]);
    }

    [Fact]
    public void PlayerConnection_BuildsStableLuaPayload()
    {
        var at = DateTimeOffset.UtcNow;

        var payload = LuaPayloadBuilder.PlayerConnection(42, "127.0.0.1:2593", at);

        Assert.Equal(42L, payload["session_id"]);
        Assert.Equal("127.0.0.1:2593", payload["remote_endpoint"]);
        Assert.Equal(at, payload["at"]);
    }

    [Fact]
    public void Timer_BuildsStableLuaPayload()
    {
        var payload = LuaPayloadBuilder.Timer("spawn", true);

        Assert.Equal("spawn", payload["name"]);
        Assert.True((bool)payload["repeat"]!);
    }
}
