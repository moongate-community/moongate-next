using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Types.Commands;
using Moongate.Network.UO.Packets.Incoming.Speech;

namespace Moongate.Server.Services.Commands;

/// <summary>
///     Converts in-game speech commands into command-system invocations.
/// </summary>
[RegisterPacketHandler]
public sealed class SpeechCommandPacketHandler
    : IPacketHandler<UnicodeSpeechPacket>,
        IPacketHandler<TalkRequestPacket>
{
    private const char CommandPrefix = '.';

    private readonly ICommandSystemService _commands;
    private readonly IPlayerSessionService _playerSessions;

    public SpeechCommandPacketHandler(
        ICommandSystemService commands,
        IPlayerSessionService playerSessions
    )
    {
        ArgumentNullException.ThrowIfNull(commands);
        ArgumentNullException.ThrowIfNull(playerSessions);

        _commands = commands;
        _playerSessions = playerSessions;
    }

    public Task HandleAsync(
        PacketContext<TalkRequestPacket> context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(context);

        return ExecuteIfCommandAsync(context.Packet.Text, context.SessionId, cancellationToken);
    }

    public Task HandleAsync(
        PacketContext<UnicodeSpeechPacket> context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(context);

        return ExecuteIfCommandAsync(context.Packet.Text, context.SessionId, cancellationToken);
    }

    private Task ExecuteIfCommandAsync(
        string text,
        long sessionId,
        CancellationToken cancellationToken
    )
    {
        if (!TryGetCommandText(text, out var commandText))
        {
            return Task.CompletedTask;
        }

        var playerSession = GetPlayerSession(sessionId);

        return _commands.ExecuteCommandAsync(
            commandText,
            CommandSourceType.InGame,
            sessionId,
            playerSession,
            cancellationToken
        );
    }

    private PlayerSession? GetPlayerSession(long sessionId)
    {
        if (_playerSessions.TryGetBySessionId(sessionId, out var session))
        {
            return session;
        }

        return null;
    }

    private static bool TryGetCommandText(string text, out string commandText)
    {
        commandText = "";

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var trimmed = text.Trim();

        if (trimmed.Length <= 1 || trimmed[0] != CommandPrefix)
        {
            return false;
        }

        commandText = trimmed[1..].Trim();

        return commandText.Length > 0;
    }
}
