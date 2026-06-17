using System.Globalization;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.Server.Data.Templates;

public sealed record MobileTemplateSummary(
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
    int LootTablesCount
)
{
    public static string FormatBody(int body)
    {
        return $"0x{body.ToString("X4", CultureInfo.InvariantCulture)}";
    }

    public static string FormatImageUrl(string id)
    {
        return $"/api/mobile-templates/{Uri.EscapeDataString(id)}/image.png";
    }

    public static MobileTemplateSummary FromDefinition(MobileTemplateDefinition template)
    {
        ArgumentNullException.ThrowIfNull(template);

        return new MobileTemplateSummary(
            template.Id,
            template.Name ?? "",
            template.Title ?? "",
            template.Body,
            FormatBody(template.Body),
            FormatImageUrl(template.Id),
            template.Gender.ToString(),
            template.Notoriety.ToString(),
            template.Karma,
            template.Fame,
            template.FactionId ?? "",
            template.Brain ?? "",
            template.IsAbstract,
            [.. template.Tags],
            template.Equipment.Count,
            template.LootTables.Count
        );
    }
}
