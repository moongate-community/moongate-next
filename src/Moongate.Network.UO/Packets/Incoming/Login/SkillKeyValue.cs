using Moongate.UO.Data.Types;
using Moongate.UO.Data.Types.Skills;

namespace Moongate.Network.UO.Packets.Incoming.Login;

/// <summary>
/// A skill/value pair sent during character creation.
/// </summary>
public readonly record struct SkillKeyValue(UOSkillName Skill, int Value);
