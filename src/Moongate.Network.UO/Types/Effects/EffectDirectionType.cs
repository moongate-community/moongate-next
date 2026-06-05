namespace Moongate.Network.UO.Types.Effects;

/// <summary>
/// Defines movement behavior for graphical effect packets.
/// </summary>
public enum EffectDirectionType : byte
{
    SourceToTarget = 0x00,
    LightningStrike = 0x01,
    StayAtLocation = 0x02,
    FollowCharacter = 0x03
}
