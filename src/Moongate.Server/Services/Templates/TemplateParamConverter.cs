using Moongate.UO.Data.Data;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Properties;

namespace Moongate.Server.Services.Templates;

/// <summary>
/// Shared conversion from a template param definition to an entity custom property,
/// used by both the item and mobile factories.
/// </summary>
public static class TemplateParamConverter
{
    public static CustomProperty ToCustomProperty(ItemTemplateParamDefinition param)
    {
        ArgumentNullException.ThrowIfNull(param);

        if (param.Type == ItemTemplateParamType.String)
        {
            return new()
            {
                Type = CustomPropertyType.String,
                StringValue = param.Value
            };
        }

        return new()
        {
            Type = CustomPropertyType.Integer,
            IntegerValue = ItemTemplateYamlLoader.ParseLong(param.Value)
        };
    }
}
