using Microsoft.Extensions.Hosting;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Server.Interfaces.Commands;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Commands;

/// <summary>
/// Reads command lines from stdin when an interactive console is available.
/// </summary>
public sealed class ConsoleCommandService : IConsoleCommandService, IDisposable
{
    internal const string Prompt = "MG> ";

    private readonly ILogger _logger = Log.ForContext<ConsoleCommandService>();
    private readonly ICommandSystemService _commands;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly Func<bool> _isInputRedirected;
    private CancellationTokenRegistration _startupRegistration;
    private CancellationTokenSource? _stop;
    private Task? _loop;

    public ConsoleCommandService(ICommandSystemService commands, IHostApplicationLifetime lifetime)
        : this(commands, lifetime, static () => Console.IsInputRedirected)
    {
    }

    internal ConsoleCommandService(
        ICommandSystemService commands,
        IHostApplicationLifetime lifetime,
        Func<bool> isInputRedirected
    )
    {
        ArgumentNullException.ThrowIfNull(commands);
        ArgumentNullException.ThrowIfNull(lifetime);
        ArgumentNullException.ThrowIfNull(isInputRedirected);

        _commands = commands;
        _lifetime = lifetime;
        _isInputRedirected = isInputRedirected;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        if (_stop is not null)
        {
            return Task.CompletedTask;
        }

        if (_isInputRedirected())
        {
            _logger.Debug("Console command service disabled because stdin is redirected.");

            return Task.CompletedTask;
        }

        _stop = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.ApplicationStopping);
        _startupRegistration = _lifetime.ApplicationStarted.Register(StartLoop);
        _logger.Information("Console command service waiting for application startup.");

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_stop is null)
        {
            return;
        }

        await _stop.CancelAsync();
        _startupRegistration.Dispose();

        if (_loop is not null)
        {
            await _loop.WaitAsync(cancellationToken);
        }
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Console.Write(Prompt);
                var line = await Console.In.ReadLineAsync(cancellationToken);

                if (line is null)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                await _commands.ExecuteCommandAsync(line, cancellationToken: cancellationToken);

                if (IsLoopTerminatingCommand(line))
                {
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Console command failed.");
            }
        }
    }

    private void StartLoop()
    {
        if (_stop is null || _stop.IsCancellationRequested || _loop is not null)
        {
            return;
        }

        _loop = RunLoopAsync(_stop.Token);
        _logger.Information("Console command service started.");
    }

    internal static bool IsLoopTerminatingCommand(string line)
    {
        ArgumentNullException.ThrowIfNull(line);

        var trimmed = line.TrimStart();

        if (trimmed.Length == 0)
        {
            return false;
        }

        var commandLength = trimmed.Length;

        for (var i = 0; i < trimmed.Length; i++)
        {
            if (char.IsWhiteSpace(trimmed[i]))
            {
                commandLength = i;

                break;
            }
        }

        var command = trimmed[..commandLength];

        return string.Equals(command, "exit", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(command, "stop", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(command, "quit", StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _startupRegistration.Dispose();
        _stop?.Dispose();
    }
}
