namespace Moongate.UO.Data.Types;

/// <summary>Lock state controlling whether a skill or stat raises, lowers, or is fixed.</summary>
public enum UOSkillLock : byte
{
    /// <summary>Value may raise.</summary>
    Up = 0,

    /// <summary>Value may lower.</summary>
    Down = 1,

    /// <summary>Value is locked.</summary>
    Locked = 2
}
