using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types.Mobiles;

namespace Moongate.Network.UO.Data;

/// <summary>
/// Computes the status-flag byte shared by the 0x77/0x78 mobile packets.
/// Only the fields currently modelled are encoded (gender); the rest are reserved for later.
/// </summary>
public static class MobilePacketFlags
{
    /// <summary>Returns the UO mobile status flags for the given mobile.</summary>
    public static byte For(MobileEntity mobile)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        byte flags = 0x00;

        if (mobile.Gender == GenderType.Female)
        {
            flags |= 0x02;
        }

        return flags;
    }
}
