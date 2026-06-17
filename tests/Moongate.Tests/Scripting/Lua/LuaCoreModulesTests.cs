using DryIoc;
using Moongate.Abstractions.Data.Timing;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Abstractions.Interfaces.Timing;
using Moongate.Abstractions.Services.Commands;
using Moongate.Abstractions.Types.Commands;
using Moongate.Scripting.Lua.Data.Internal;
using Moongate.Scripting.Lua.Interfaces.Events;
using Moongate.Scripting.Lua.Modules;
using Moongate.Scripting.Lua.Services;
using Moongate.Server.Services.Commands;
using Moongate.Server.Services.Timing;
using Moongate.Tests.Scripting.Lua.Support;

namespace Moongate.Tests.Scripting.Lua;

public class LuaCoreModulesTests
{
    [Fact]
    public async Task Commands_Execute_RunsRegisteredCommand()
    {
        using var fixture = NewFixture(
            typeof(CommandsModule),
            container =>
            {
                container.Register<ICommandRegistry, CommandRegistry>(Reuse.Singleton);
                container.Register<ICommandSystemService, CommandSystemService>(Reuse.Singleton);
            }
        );
        var registry = fixture.Container.Resolve<ICommandRegistry>();
        registry.RegisterCommand(
            "server_echo",
            context =>
            {
                context.Print("server {0}", context.Arguments[0]);

                return Task.CompletedTask;
            },
            source: CommandSourceType.All
        );

        await fixture.Engine.StartAsync();
        fixture.Engine.ExecuteScript(
            """
            command_output = commands.execute("server_echo moon", "console")
            command_exists = commands.exists("server_echo")
            """
        );

        Assert.Equal("server moon", fixture.Engine.ExecuteFunction("command_output").Data);
        Assert.True((bool)fixture.Engine.ExecuteFunction("command_exists").Data!);
    }

    [Fact]
    public async Task Commands_Register_CreatesLuaBackedCommand()
    {
        using var fixture = NewFixture(
            typeof(CommandsModule),
            container =>
            {
                container.Register<ICommandRegistry, CommandRegistry>(Reuse.Singleton);
                container.Register<ICommandSystemService, CommandSystemService>(Reuse.Singleton);
            }
        );

        await fixture.Engine.StartAsync();
        fixture.Engine.ExecuteScript(
            """
            captured_source = nil
            captured_arg = nil
            commands.register("lua_echo", "all", "Echoes Lua arguments.", function(ctx)
                captured_source = ctx.source
                captured_arg = ctx.args[1]
                return ctx.args[1] .. " " .. ctx.args[2]
            end)
            """
        );

        var commands = fixture.Container.Resolve<ICommandSystemService>();
        var output = await commands.ExecuteCommandWithOutputAsync("lua_echo hello Britannia");

        Assert.Equal(["hello Britannia"], output);
        Assert.Equal("Console", fixture.Engine.ExecuteFunction("captured_source").Data);
        Assert.Equal("hello", fixture.Engine.ExecuteFunction("captured_arg").Data);
    }

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
    {
        return NewFixture(modules, null);
    }

    private static LuaEngineFixture NewFixture(Type module, Action<IContainer> configure)
    {
        return NewFixture([module], configure);
    }

    private static LuaEngineFixture NewFixture(Type[] modules, Action<IContainer>? configure)
    {
        return new LuaEngineFixture(
            modules.Select(module => new ScriptModuleData(module)),
            container =>
            {
                container.Register<ILuaEventBridge, LuaEventBridge>(Reuse.Singleton);
                configure?.Invoke(container);
            }
        );
    }

    private static TimerWheelService NewTimer()
    {
        return new TimerWheelService(new TimerWheelConfig { TickDuration = TimeSpan.FromMilliseconds(8), WheelSize = 64 });
    }
}
