using MoonSharp.Interpreter;
using Moongate.Core.Utils;
using Moongate.Scripting.Lua.Data.Scripts;
using Moongate.Tests.Scripting.Lua.Support;

namespace Moongate.Tests.Scripting.Lua;

public class LuaScriptEngineServiceTests
{
    [Fact]
    public void AddCallback_EmptyName_Throws()
    {
        using var fixture = new LuaEngineFixture();

        Assert.ThrowsAny<ArgumentException>(() => fixture.Engine.AddCallback("", _ => { }));
    }

    [Fact]
    public void AddCallback_ExecuteCallback_InvokesRegisteredAction()
    {
        using var fixture = new LuaEngineFixture();
        object[]? received = null;
        fixture.Engine.AddCallback("onHit", args => received = args);

        fixture.Engine.ExecuteCallback("onHit", 7, "crit");

        Assert.NotNull(received);
        Assert.Equal(new object[] { 7, "crit" }, received);
    }

    [Fact]
    public void AddConstant_ExposesValueAsUpperSnakeCaseGlobal()
    {
        using var fixture = new LuaEngineFixture();

        fixture.Engine.AddConstant("maxPlayers", 100);

        var result = fixture.Engine.ExecuteFunction("MAX_PLAYERS");
        Assert.True(result.Success);
        Assert.Equal(100d, Assert.IsType<double>(result.Data));
    }

    [Fact]
    public void ExecuteCallback_UnknownName_DoesNotThrow()
    {
        using var fixture = new LuaEngineFixture();

        fixture.Engine.ExecuteCallback("never_registered");
    }

    [Fact]
    public void ExecuteFunction_ArithmeticExpression_ReturnsSuccessWithData()
    {
        using var fixture = new LuaEngineFixture();

        var result = fixture.Engine.ExecuteFunction("1 + 2");

        Assert.True(result.Success);
        Assert.Equal(3d, Assert.IsType<double>(result.Data));
    }

    [Fact]
    public void ExecuteFunction_InvalidLua_ReturnsErrorResultWithoutThrowing()
    {
        using var fixture = new LuaEngineFixture();

        var result = fixture.Engine.ExecuteFunction("nonexistent_fn()");

        Assert.False(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    [Fact]
    public void ExecuteFunction_ReadsRegisteredGlobal()
    {
        using var fixture = new LuaEngineFixture();
        fixture.Engine.RegisterGlobal("answer", 42);

        var result = fixture.Engine.ExecuteFunction("answer");

        Assert.True(result.Success);
        Assert.Equal(42d, Assert.IsType<double>(result.Data));
    }

    [Fact]
    public void ExecuteScript_IdenticalScriptTwice_CountsCacheHit()
    {
        using var fixture = new LuaEngineFixture();

        fixture.Engine.ExecuteScript("x = 1");
        fixture.Engine.ExecuteScript("x = 1");

        var metrics = fixture.Engine.GetExecutionMetrics();
        Assert.Equal(1, metrics.CacheMisses);
        Assert.Equal(1, metrics.CacheHits);
        Assert.Equal(1, metrics.TotalScriptsCached);
    }

    [Fact]
    public void ExecuteScript_RaisesOnScriptErrorEvent()
    {
        using var fixture = new LuaEngineFixture();
        ScriptErrorInfo? captured = null;
        fixture.Engine.OnScriptError += (_, info) => captured = info;

        Assert.ThrowsAny<InterpreterException>(() => fixture.Engine.ExecuteScript("error('boom')"));

        Assert.NotNull(captured);
    }

    [Fact]
    public void ExecuteScript_RuntimeError_Throws()
    {
        using var fixture = new LuaEngineFixture();

        Assert.ThrowsAny<InterpreterException>(() => fixture.Engine.ExecuteScript("error('boom')"));
    }

    [Fact]
    public void ExecuteScript_ValidScript_MutatesGlobalState()
    {
        using var fixture = new LuaEngineFixture();

        fixture.Engine.ExecuteScript("computed = 10 * 5");

        var result = fixture.Engine.ExecuteFunction("computed");
        Assert.Equal(50d, Assert.IsType<double>(result.Data));
    }

    [Fact]
    public void RegisterGlobal_NullValue_Throws()
    {
        using var fixture = new LuaEngineFixture();

        Assert.Throws<ArgumentNullException>(() => fixture.Engine.RegisterGlobal("x", null!));
    }

    [Fact]
    public void RegisterGlobalFunction_CallableFromLua()
    {
        using var fixture = new LuaEngineFixture();
        fixture.Engine.RegisterGlobalFunction("double_it", (int x) => x * 2);

        var result = fixture.Engine.ExecuteFunction("double_it(21)");

        Assert.True(result.Success);
        Assert.Equal(42d, Assert.IsType<double>(result.Data));
    }

    [Fact]
    public async Task StartAsync_RegistersRuntimeMetadataConstants()
    {
        using var fixture = new LuaEngineFixture();

        await fixture.Engine.StartAsync();

        Assert.Equal(VersionUtils.GetVersion(), fixture.Engine.ExecuteFunction("VERSION").Data);
        Assert.Equal("Moongate", fixture.Engine.ExecuteFunction("ENGINE").Data);
        Assert.Equal(PlatformUtils.GetCurrentPlatform().ToString(), fixture.Engine.ExecuteFunction("PLATFORM").Data);
    }

    [Theory, InlineData("MyFunction", "my_function"), InlineData("DoThing", "do_thing")]
    public void ToScriptEngineFunctionName_ConvertsToSnakeCase(string input, string expected)
    {
        using var fixture = new LuaEngineFixture();

        Assert.Equal(expected, fixture.Engine.ToScriptEngineFunctionName(input));
    }

    [Fact]
    public void UnregisterGlobal_ExistingGlobal_RemovesItAndReturnsTrue()
    {
        using var fixture = new LuaEngineFixture();
        fixture.Engine.RegisterGlobal("temp", 1);

        var removed = fixture.Engine.UnregisterGlobal("temp");

        Assert.True(removed);
        var result = fixture.Engine.ExecuteFunction("temp");
        Assert.Null(result.Data);
    }

    [Fact]
    public void UnregisterGlobal_UnknownGlobal_ReturnsFalse()
    {
        using var fixture = new LuaEngineFixture();

        Assert.False(fixture.Engine.UnregisterGlobal("never_set"));
    }
}
