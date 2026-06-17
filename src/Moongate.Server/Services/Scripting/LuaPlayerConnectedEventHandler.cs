using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Scripting.Lua.Interfaces.Events;
using Moongate.Scripting.Lua.Utils;
using Moongate.Server.Data.Events;

namespace Moongate.Server.Services.Scripting;

public sealed class LuaPlayerConnectedEventHandler : ITickEventHandler<PlayerConnectedEvent>
{
    private readonly ILuaEventBridge _events;

    public LuaPlayerConnectedEventHandler(ILuaEventBridge events)
    {
        _events = events;
    }

    public void Handle(PlayerConnectedEvent evt)
    {
        _events.Publish(
            "player.connected",
            LuaPayloadBuilder.PlayerConnection(evt.SessionId, evt.RemoteEndPoint, evt.At)
        );
    }
}
