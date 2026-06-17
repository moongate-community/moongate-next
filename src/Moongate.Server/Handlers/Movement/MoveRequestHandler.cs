using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Abstractions.Types.Player;
using Moongate.Core.Geometry;
using Moongate.Core.Types;
using Moongate.Network.UO.Packets.Incoming.Movement;
using Moongate.Network.UO.Packets.Outgoing.Movement;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services.Movement;
using Moongate.Server.Interfaces.Services.World;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Server.Handlers.Movement;

/// <summary>
/// Handles movement requests (0x02): sequence sync, fastwalk throttle, turn-then-step, validation,
/// and the matching confirm (0x22) or deny (0x21) reply.
/// </summary>
[RegisterPacketHandler]
public class MoveRequestHandler : PacketHandlerBase<MoveRequestPacket>
{
    private const long MovementThrottleResetMs = 1000;
    private const long MovementThrottleThresholdMs = 400;
    private const int WalkFootDelayMs = 400;
    private const int RunFootDelayMs = 200;
    private const int TurnDelayMs = 100;

    private readonly IWorldSpatialIndex _index;
    private readonly IMovementValidationService _validation;

    public MoveRequestHandler(
        IEventBusService eventBus,
        INetworkSessionManager sessions,
        IPlayerSessionService playerSessions,
        IWorldSpatialIndex index,
        IMovementValidationService validation
    ) : base(eventBus, sessions, playerSessions)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(validation);

        _index = index;
        _validation = validation;
    }

    public override async Task HandleAsync(
        PacketContext<MoveRequestPacket> context,
        CancellationToken cancellationToken = default
    )
    {
        if (!PlayerSessions.TryGetBySessionId(context.SessionId, out var session) ||
            session.State != PlayerSessionStateType.InWorld ||
            session.MobileSerial is not { } serial ||
            !_index.TryGet(serial, out var mobile))
        {
            return;
        }

        var packet = context.Packet;

        if (session.MoveSequence == 0 && packet.Sequence != 0)
        {
            return; // resync: drop without replying
        }

        if (IsThrottled(session))
        {
            PlayerSessions.UpdateMovementState(context.SessionId, session.MoveSequence, session.MoveCredit, session.MoveTime);
            await SendDenyAsync(context, mobile, packet.Sequence, cancellationToken);

            return;
        }

        var baseDirection = Point3D.GetBaseDirection(mobile.Direction);
        var isTurnOnly = baseDirection != packet.WalkDirection;

        if (isTurnOnly)
        {
            mobile.Direction = packet.WalkDirection;
        }
        else
        {
            if (!_validation.TryResolveMove(mobile, packet.Direction, out var newLocation))
            {
                session.MoveSequence = 0;
                PlayerSessions.UpdateMovementState(context.SessionId, session.MoveSequence, session.MoveCredit, session.MoveTime);
                await SendDenyAsync(context, mobile, packet.Sequence, cancellationToken);

                return;
            }

            var oldLocation = mobile.Location;
            _index.MoveMobile(mobile, newLocation);
            mobile.Direction = packet.Direction;

            EventBus.Publish(new MobileMovedEvent(mobile.Id, mobile.MapId, oldLocation, newLocation, packet.Direction));
        }

        var nextSequence = packet.Sequence + 1;
        if (nextSequence == 256)
        {
            nextSequence = 1;
        }

        session.MoveSequence = (byte)nextSequence;

        await context.SendAsync(new MoveConfirmPacket(packet.Sequence, (byte)mobile.Notoriety), cancellationToken);

        session.MoveTime += isTurnOnly ? TurnDelayMs : ComputeSpeedMs(packet.Direction);

        PlayerSessions.UpdateMovementState(context.SessionId, session.MoveSequence, session.MoveCredit, session.MoveTime);
    }

    private static int ComputeSpeedMs(DirectionType direction)
        => (direction & DirectionType.Running) != 0 ? RunFootDelayMs : WalkFootDelayMs;

    private static bool IsThrottled(PlayerSession session)
    {
        var now = Environment.TickCount64;
        var nextMove = session.MoveTime;

        if (now - nextMove - MovementThrottleResetMs > 0)
        {
            session.MoveCredit = 0;
            session.MoveTime = now;

            return false;
        }

        var cost = nextMove - now;
        if (session.MoveCredit < cost)
        {
            return true;
        }

        session.MoveCredit = Math.Min(MovementThrottleThresholdMs, session.MoveCredit - cost);

        return false;
    }

    private static Task SendDenyAsync(
        PacketContext<MoveRequestPacket> context,
        MobileEntity mobile,
        byte sequence,
        CancellationToken cancellationToken
    )
        => context.SendAsync(
            new MoveDenyPacket(
                sequence,
                (short)mobile.Location.X,
                (short)mobile.Location.Y,
                mobile.Direction,
                (sbyte)mobile.Location.Z
            ),
            cancellationToken
        );
}
