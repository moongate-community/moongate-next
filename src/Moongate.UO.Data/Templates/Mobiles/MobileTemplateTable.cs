namespace Moongate.UO.Data.Templates.Mobiles;

/// <summary>
///     Root YAML document for mobile template files (<c>mobile_templates</c> key).
/// </summary>
public sealed class MobileTemplateTable
{
    public List<MobileTemplateDefinition> MobileTemplates { get; set; } = [];
}
