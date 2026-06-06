using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Abstractions.Types.Commands;
using Moongate.Scripting.Lua.Attributes.Scripts;
using Moongate.Scripting.Lua.Interfaces.Events;
using Moongate.Scripting.Lua.Utils;
using MoonSharp.Interpreter;

namespace Moongate.Scripting.Lua.Modules;

[ScriptModule("commands", "Allows Lua scripts to register and execute server commands.")]
public sealed class CommandsModule
{
    private readonly ICommandRegistry? _registry;
    private readonly ICommandSystemService? _commands;
    private readonly ILuaEventBridge _events;

    public CommandsModule(
        ILuaEventBridge events,
        ICommandRegistry? registry = null,
        ICommandSystemService? commands = null
    )
    {
        ArgumentNullException.ThrowIfNull(events);

        _events = events;
        _registry = registry;
        _commands = commands;
    }

    [ScriptFunction("execute", "Executes a registered server command and returns captured output.")]
    public string Execute(string commandWithArgs, string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandWithArgs);

        var commands = GetCommandSystem();
        var commandSource = ParseSource(source, CommandSourceType.Console);
        var output = commands.ExecuteCommandWithOutputAsync(commandWithArgs, commandSource)
                             .GetAwaiter()
                             .GetResult();

        return string.Join(Environment.NewLine, output);
    }

    [ScriptFunction("exists", "Returns true when a command or alias is registered.")]
    public bool Exists(string commandName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);

        return GetRegistry().TryGetCommand(commandName, out _);
    }

    [ScriptFunction("register", "Registers a Lua-backed command.")]
    public void Register(string commandName, string source, string description, Closure callback)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        ArgumentNullException.ThrowIfNull(callback);

        var registry = GetRegistry();
        var commandSource = ParseSource(source, CommandSourceType.All);

        registry.RegisterCommand(
            commandName,
            context =>
            {
                var result = _events.Invoke(callback, LuaPayloadBuilder.Command(context));
                var output = GetOutput(result);

                if (!string.IsNullOrWhiteSpace(output))
                {
                    context.Print(output);
                }

                return Task.CompletedTask;
            },
            description ?? "",
            commandSource
        );
    }

    private static string? GetOutput(DynValue result)
        => result.Type switch
        {
            DataType.Nil     => null,
            DataType.Void    => null,
            DataType.String  => result.String,
            DataType.Number  => result.Number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            DataType.Boolean => result.Boolean.ToString(),
            _                => result.ToObject()?.ToString()
        };

    private static CommandSourceType ParseSource(string? source, CommandSourceType defaultSource)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return defaultSource;
        }

        return source.Trim().ToLowerInvariant() switch
        {
            "*" or "all"                 => CommandSourceType.All,
            "console"                    => CommandSourceType.Console,
            "game" or "ingame"           => CommandSourceType.InGame,
            "in_game" or "in-game"       => CommandSourceType.InGame,
            _                            => throw new ScriptRuntimeException($"Invalid command source '{source}'.")
        };
    }

    private ICommandRegistry GetRegistry()
        => _registry ?? throw new ScriptRuntimeException("Command registry is not registered.");

    private ICommandSystemService GetCommandSystem()
        => _commands ?? throw new ScriptRuntimeException("Command system is not registered.");
}
