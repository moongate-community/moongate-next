using DryIoc;
using Moongate.Abstractions.Interfaces.Timing;
using Moongate.Scripting.Lua.Data.Internal;
using Moongate.Scripting.Lua.Interfaces.Events;
using Moongate.Scripting.Lua.Modules;
using Moongate.Scripting.Lua.Services;
using Moongate.Server.Services.Timing;
using Moongate.Tests.Scripting.Lua.Support;

namespace Moongate.Tests.Scripting.Lua;

public class LuaCoreModulesTests
{
    [Fact]
    public async Task Events_On_InvokesRegisteredCallback()
    {
        using var fixture = NewFixture(typeof(EventsModule));

        await fixture.Engine.StartAsync();
        fixture.Engine.ExecuteScript(
            """
            captured_session = nil
            events.on("player.connected", function(ctx)
                captured_session = ctx.session_id
            end)
            """
        );

        var bridge = fixture.Container.Resolve<ILuaEventBridge>();
        bridge.Publish(
            "player.connected",
            new Dictionary<string, object?>
            {
                ["session_id"] = 42
            }
        );

        var result = fixture.Engine.ExecuteFunction("captured_session");
        Assert.True(result.Success);
        Assert.Equal(42d, Assert.IsType<double>(result.Data));
    }

    [Fact]
    public async Task RandomModule_ExposesExpectedHelpers()
    {
        using var fixture = NewFixture(typeof(RandomModule));

        await fixture.Engine.StartAsync();

        Assert.False((bool)fixture.Engine.ExecuteFunction("random.chance(0)").Data!);
        Assert.True((bool)fixture.Engine.ExecuteFunction("random.chance(100)").Data!);

        var integer = Assert.IsType<double>(fixture.Engine.ExecuteFunction("random.int(1, 3)").Data);
        Assert.InRange(integer, 1, 3);

        var picked = fixture.Engine.ExecuteFunction("random.pick({ 'a' })");
        Assert.True(picked.Success);
        Assert.Equal("a", picked.Data);
    }

    [Fact]
    public async Task Timers_Once_InvokesCallbackAfterTimerTick()
    {
        var timer = NewTimer();
        using var fixture = NewFixture(typeof(TimersModule), container => container.RegisterInstance<ITimerService>(timer));

        await fixture.Engine.StartAsync();
        fixture.Engine.ExecuteScript(
            """
            fired_timer = nil
            timers.once("lua_once", "00:00:00.008", function(ctx)
                fired_timer = ctx.name
            end)
            """
        );

        timer.UpdateTicksDelta(0);
        timer.UpdateTicksDelta(8);

        var result = fixture.Engine.ExecuteFunction("fired_timer");
        Assert.True(result.Success);
        Assert.Equal("lua_once", result.Data);
    }

    private static LuaEngineFixture NewFixture(params Type[] modules)
        => NewFixture(modules, null);

    private static LuaEngineFixture NewFixture(Type module, Action<IContainer> configure)
        => NewFixture([module], configure);

    private static LuaEngineFixture NewFixture(Type[] modules, Action<IContainer>? configure)
        => new(
            modules.Select(module => new ScriptModuleData(module)),
            container =>
            {
                container.Register<ILuaEventBridge, LuaEventBridge>(Reuse.Singleton);
                configure?.Invoke(container);
            }
        );

    private static TimerWheelService NewTimer()
        => new(new() { TickDuration = TimeSpan.FromMilliseconds(8), WheelSize = 64 });
}
