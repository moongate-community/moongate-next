using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Scripting.Lua.Interfaces.Events;
using Moongate.Scripting.Lua.Utils;
using Moongate.Server.Data.Events;

namespace Moongate.Server.Services.Scripting;

public sealed class LuaPlayerDisconnectedEventHandler : ITickEventHandler<PlayerDisconnectedEvent>
{
    private readonly ILuaEventBridge _events;

    public LuaPlayerDisconnectedEventHandler(ILuaEventBridge events)
    {
        _events = events;
    }

    public void Handle(PlayerDisconnectedEvent evt)
    {
        _events.Publish(
            "player.disconnected",
            LuaPayloadBuilder.PlayerConnection(evt.SessionId, evt.RemoteEndPoint, evt.At)
        );
    }
}
