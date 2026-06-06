using DryIoc;
using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Types.Commands;
using Serilog.Events;

namespace Moongate.Abstractions.Data.Commands;

/// <summary>
/// Carries parsed command state and output helpers for command handlers.
/// </summary>
public sealed class CommandSystemContext
{
    private readonly Action<string, LogEventLevel> _printAction;

    public CommandSystemContext(
        string commandText,
        IReadOnlyList<string> arguments,
        CommandSourceType source,
        IServiceProvider services,
        Action<string, LogEventLevel> printAction,
        long? sessionId = null,
        PlayerSession? playerSession = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandText);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(printAction);

        CommandText = commandText;
        Arguments = arguments;
        Source = source;
        Services = services;
        _printAction = printAction;
        SessionId = sessionId;
        PlayerSession = playerSession;
    }

    /// <summary>
    /// Raw command text without source-specific prefixes.
    /// </summary>
    public string CommandText { get; }

    /// <summary>
    /// Parsed command arguments.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; }

    /// <summary>
    /// Command source.
    /// </summary>
    public CommandSourceType Source { get; }

    /// <summary>
    /// Runtime service provider available to command handlers.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Network session id for in-game commands.
    /// </summary>
    public long? SessionId { get; }

    /// <summary>
    /// Logical player session for in-game commands, when known.
    /// </summary>
    public PlayerSession? PlayerSession { get; }

    /// <summary>
    /// Whether this command was executed from the game client.
    /// </summary>
    public bool IsInGame => Source == CommandSourceType.InGame;

    /// <summary>
    /// Writes informational command output.
    /// </summary>
    public void Print(string message, params object[] args)
        => Print(LogEventLevel.Information, message, args);

    /// <summary>
    /// Writes error command output.
    /// </summary>
    public void PrintError(string message, params object[] args)
        => Print(LogEventLevel.Error, message, args);

    /// <summary>
    /// Writes warning command output.
    /// </summary>
    public void PrintWarning(string message, params object[] args)
        => Print(LogEventLevel.Warning, message, args);

    /// <summary>
    /// Resolves a service from the command service provider.
    /// </summary>
    /// <typeparam name="T">Service type.</typeparam>
    /// <returns>The resolved service.</returns>
    public T Resolve<T>()
        where T : notnull
    {
        if (Services is IResolverContext resolverContext)
        {
            return resolverContext.Resolve<T>();
        }

        return (T)(Services.GetService(typeof(T)) ??
                   throw new InvalidOperationException($"Service {typeof(T).FullName} is not registered."));
    }

    private void Print(LogEventLevel level, string message, params object[] args)
    {
        var formatted = args.Length == 0 ? message : string.Format(message, args);
        _printAction(formatted, level);
    }
}
