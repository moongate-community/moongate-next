using System.Text;
using Moongate.Abstractions.Data.Commands;
using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Abstractions.Types.Commands;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Commands;

/// <summary>
/// Dispatches parsed command lines to registered handlers.
/// </summary>
public sealed class CommandSystemService : ICommandSystemService
{
    private readonly ILogger _logger = Log.ForContext<CommandSystemService>();
    private readonly ICommandRegistry _registry;
    private readonly IServiceProvider _services;

    public CommandSystemService(ICommandRegistry registry, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(services);

        _registry = registry;
        _services = services;
    }

    public Task ExecuteCommandAsync(
        string commandWithArgs,
        CommandSourceType source = CommandSourceType.Console,
        long? sessionId = null,
        PlayerSession? playerSession = null,
        CancellationToken cancellationToken = default
    )
        => ExecuteCommandWithOutputAsync(commandWithArgs, source, sessionId, playerSession, cancellationToken);

    public async Task<IReadOnlyList<string>> ExecuteCommandWithOutputAsync(
        string commandWithArgs,
        CommandSourceType source = CommandSourceType.Console,
        long? sessionId = null,
        PlayerSession? playerSession = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(commandWithArgs);

        var output = new List<string>();
        var tokens = ParseTokens(commandWithArgs);

        if (tokens.Count == 0)
        {
            return output;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var commandName = tokens[0];

        if (!_registry.TryGetCommand(commandName, out var definition))
        {
            WriteOutput(output, LogEventLevel.Warning, "Unknown command '{0}'.", commandName);

            return output;
        }

        if ((definition.Source & source) == 0)
        {
            WriteOutput(
                output,
                LogEventLevel.Warning,
                "Command '{0}' is not available from {1}.",
                definition.Name,
                source
            );

            return output;
        }

        var context = new CommandSystemContext(
            commandWithArgs,
            tokens.Skip(1).ToArray(),
            source,
            _services,
            (message, level) => WriteOutput(output, level, "{0}", message),
            sessionId,
            playerSession
        );

        await definition.Handler(context);

        return output;
    }

    public IReadOnlyList<string> GetAutocompleteSuggestions(string commandWithArgs)
    {
        ArgumentNullException.ThrowIfNull(commandWithArgs);

        var endsWithWhitespace = commandWithArgs.Length > 0 &&
                                 char.IsWhiteSpace(commandWithArgs[^1]);
        var tokens = ParseTokens(commandWithArgs);

        if (tokens.Count == 0)
        {
            return _registry.GetRegisteredCommands()
                            .Select(static command => command.Name)
                            .ToArray();
        }

        if (tokens.Count == 1 && !endsWithWhitespace)
        {
            return _registry.GetRegisteredCommands()
                            .SelectMany(static command => command.Aliases)
                            .Where(alias => alias.StartsWith(tokens[0], StringComparison.OrdinalIgnoreCase))
                            .Order(StringComparer.OrdinalIgnoreCase)
                            .ToArray();
        }

        if (!_registry.TryGetCommand(tokens[0], out var definition) ||
            definition.AutocompleteProvider is null)
        {
            return [];
        }

        var arguments = tokens.Skip(1).ToArray();
        var context = new CommandAutocompleteContext
        {
            CommandName = definition.Name,
            Arguments = arguments,
            EndsWithWhitespace = endsWithWhitespace
        };

        return definition.AutocompleteProvider(context);
    }

    public IReadOnlyList<CommandDefinition> GetRegisteredCommands()
        => _registry.GetRegisteredCommands();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        return Task.CompletedTask;
    }

    private static void AddToken(List<string> tokens, StringBuilder current)
    {
        if (current.Length == 0)
        {
            return;
        }

        tokens.Add(current.ToString());
        current.Clear();
    }

    private static IReadOnlyList<string> ParseTokens(string commandWithArgs)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        var escaping = false;

        foreach (var character in commandWithArgs)
        {
            if (escaping)
            {
                current.Append(character);
                escaping = false;

                continue;
            }

            if (character == '\\')
            {
                escaping = true;

                continue;
            }

            if (character == '"')
            {
                inQuotes = !inQuotes;

                continue;
            }

            if (char.IsWhiteSpace(character) && !inQuotes)
            {
                AddToken(tokens, current);

                continue;
            }

            current.Append(character);
        }

        if (escaping)
        {
            current.Append('\\');
        }

        AddToken(tokens, current);

        return tokens;
    }

    private void WriteOutput(List<string> output, LogEventLevel level, string message, params object[] args)
    {
        var formatted = args.Length == 0 ? message : string.Format(message, args);

        output.Add(formatted);
        _logger.Write(level, "{CommandOutput}", formatted);
    }
}
