using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.Server.Data.Templates;

public sealed record MobileStatsSummary(int Strength, int Dexterity, int Intelligence);

public sealed record MobileResourcesSummary(int Hits, int Mana, int Stamina);

public sealed record MobileResistancesSummary(int Physical, int Fire, int Cold, int Poison, int Energy);

public sealed record MobileSkillSummary(string Name, int Value);

public sealed record MobileTemplateParamSummary(string Key, string Type, string Value);

public sealed record MobileTemplateDetail(
    string Id,
    string Name,
    string Title,
    int Body,
    string BodyHex,
    string ImageUrl,
    string Gender,
    string Notoriety,
    int Karma,
    int Fame,
    string FactionId,
    string Brain,
    bool IsAbstract,
    IReadOnlyList<string> Tags,
    int EquipmentCount,
    int LootTablesCount,
    string? BaseMobile,
    int RaceIndex,
    int SkinHue,
    int HairHue,
    int HairStyle,
    int FacialHairHue,
    int FacialHairStyle,
    MobileStatsSummary? Stats,
    MobileResourcesSummary? Resources,
    MobileResistancesSummary? Resistances,
    IReadOnlyList<MobileSkillSummary> Skills,
    IReadOnlyList<string> Equipment,
    string? BackpackTemplate,
    IReadOnlyList<string> LootTables,
    IReadOnlyList<MobileTemplateParamSummary> Params
)
{
    public static MobileTemplateDetail FromDefinition(MobileTemplateDefinition template)
    {
        ArgumentNullException.ThrowIfNull(template);

        var summary = MobileTemplateSummary.FromDefinition(template);

        var skills = template.Skills
                             .OrderBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                             .Select(static pair => new MobileSkillSummary(pair.Key, pair.Value))
                             .ToArray();

        var parameters = template.Params
                                 .OrderBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                                 .Select(static pair => new MobileTemplateParamSummary(
                                     pair.Key,
                                     pair.Value.Type.ToString(),
                                     pair.Value.Value
                                 ))
                                 .ToArray();

        return new(
            summary.Id,
            summary.Name,
            summary.Title,
            summary.Body,
            summary.BodyHex,
            summary.ImageUrl,
            summary.Gender,
            summary.Notoriety,
            summary.Karma,
            summary.Fame,
            summary.FactionId,
            summary.Brain,
            summary.IsAbstract,
            summary.Tags,
            summary.EquipmentCount,
            summary.LootTablesCount,
            template.BaseMobile,
            template.RaceIndex,
            template.SkinHue,
            template.HairHue,
            template.HairStyle,
            template.FacialHairHue,
            template.FacialHairStyle,
            template.Stats is null
                ? null
                : new MobileStatsSummary(template.Stats.Strength, template.Stats.Dexterity, template.Stats.Intelligence),
            template.Resources is null
                ? null
                : new MobileResourcesSummary(template.Resources.Hits, template.Resources.Mana, template.Resources.Stamina),
            template.Resistances is null
                ? null
                : new MobileResistancesSummary(
                    template.Resistances.Physical,
                    template.Resistances.Fire,
                    template.Resistances.Cold,
                    template.Resistances.Poison,
                    template.Resistances.Energy
                ),
            skills,
            [.. template.Equipment.Select(static entry => entry.Item)],
            template.BackpackTemplate,
            [.. template.LootTables],
            parameters
        );
    }
}
