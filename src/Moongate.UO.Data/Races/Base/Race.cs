using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Moongate.Core.Extensions.Strings;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types.Mobiles;

namespace Moongate.UO.Data.Races.Base;

/// <summary>
/// A playable race: identity, body graphics per gender/alive state, and appearance
/// (skin/hair) clamping and randomization. Instances live in a process-global registry
/// populated at boot; the mobile entity persists only its <c>RaceIndex</c>.
/// </summary>
public abstract class Race : ISpanParsable<Race>
{
    public const int AllowAllRaces = 0x7;
    public const int AllowHumanOrElves = 0x3;
    public const int AllowElvesOnly = 0x2;
    public const int AllowGargoylesOnly = 0x4;

    protected Race(
        int raceID,
        int raceIndex,
        string name,
        string pluralName,
        int maleBody,
        int femaleBody,
        int maleGhostBody,
        int femaleGhostBody
    )
    {
        RaceID = raceID;
        RaceIndex = raceIndex;
        RaceFlag = 1 << raceIndex;
        Name = name;
        PluralName = pluralName;
        MaleBody = maleBody;
        FemaleBody = femaleBody;
        MaleGhostBody = maleGhostBody;
        FemaleGhostBody = femaleGhostBody;
    }

    public static Race[] Races { get; } = new Race[0x100];

    public static List<Race> AllRaces { get; } = new();

    public static Race DefaultRace => Races[0];
    public static Race Human => Races[0];
    public static Race Elf => Races[1];
    public static Race Gargoyle => Races[2];

    public int MaleBody { get; }
    public int MaleGhostBody { get; }
    public int FemaleBody { get; }
    public int FemaleGhostBody { get; }
    public int RaceID { get; }
    public int RaceIndex { get; }
    public int RaceFlag { get; }
    public string Name { get; set; }
    public string PluralName { get; set; }

    public virtual int AliveBody(MobileEntity mobile)
        => AliveBody(mobile.Gender == GenderType.Female);

    public virtual int AliveBody(bool female)
        => female ? FemaleBody : MaleBody;

    public virtual int Body(MobileEntity mobile)
        => mobile.IsAlive ? AliveBody(mobile.Gender == GenderType.Female) : GhostBody(mobile.Gender == GenderType.Female);

    public abstract int ClipHairHue(int hue);

    public abstract int ClipSkinHue(int hue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Race? GetRace(int raceID)
        => AllRaces.FirstOrDefault(r => r.RaceID == raceID);

    public virtual int GhostBody(MobileEntity mobile)
        => GhostBody(mobile.Gender == GenderType.Female);

    public virtual int GhostBody(bool female)
        => female ? FemaleGhostBody : MaleGhostBody;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAllowedRace(Race race, int allowedRaceFlags)
        => (allowedRaceFlags & race.RaceFlag) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Race Parse(string s)
        => Parse(s, null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Race Parse(string s, IFormatProvider? provider)
        => Parse(s.AsSpan(), provider);

    public static Race Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var race))
        {
            return race;
        }

        throw new FormatException($"The input string '{s}' was not in a correct format.");
    }

    public abstract int RandomFacialHair(bool female);

    public abstract int RandomHair(bool female);

    public abstract int RandomHairHue();

    public abstract int RandomSkinHue();

    public override string ToString()
        => Name;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Race result)
        => TryParse(s.AsSpan(), provider, out result);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Race result)
    {
        if (int.TryParse(s, out var index) && index >= 0 && index < Races.Length && Races[index] is not null)
        {
            result = Races[index];

            return true;
        }

        var trimmed = s.Trim();

        foreach (var race in AllRaces)
        {
            if (trimmed.InsensitiveEquals(race.Name) || trimmed.InsensitiveEquals(race.PluralName))
            {
                result = race;

                return true;
            }
        }

        result = null;

        return false;
    }

    public abstract bool ValidateFacialHair(bool female, int itemID);

    public abstract bool ValidateHair(bool female, int itemID);
}
