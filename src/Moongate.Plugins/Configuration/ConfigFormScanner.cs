using System.Collections.Concurrent;
using System.Reflection;
using Moongate.Abstractions.Configuration;
using Moongate.Plugins.Data;
using Moongate.Plugins.Types;

namespace Moongate.Plugins.Configuration;

/// <summary>
/// Builds a <see cref="PluginConfigForm" /> by reflecting over <see cref="ConfigSectionAttribute" /> and
/// <see cref="ConfigFieldAttribute" /> on a typed config object. The structural schema is cached per type
/// on first scan; current values are projected from the supplied instance on every call.
/// </summary>
public static class ConfigFormScanner
{
    private static readonly ConcurrentDictionary<Type, FormSchema> _cache = new();

    private sealed record FormSchema(IReadOnlyList<SectionSchema> Sections);

    private sealed record SectionSchema(string Id, string Label, int Order, List<FieldSchema> Fields);

    private sealed record FieldSchema(
        string Path,
        string Label,
        string Type,
        bool Required,
        string? Help,
        IReadOnlyList<string> Options,
        bool Secret,
        int Order,
        PropertyInfo[] Chain,
        object? DefaultValue
    );

    /// <summary>Builds the config form for <paramref name="instance" />'s type, filled with its current values.</summary>
    public static PluginConfigForm BuildForm<T>(T instance)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(instance);

        var schema = _cache.GetOrAdd(typeof(T), BuildSchema);
        var sections = new List<PluginConfigSection>(schema.Sections.Count);

        foreach (var section in schema.Sections)
        {
            var fields = new List<PluginConfigField>(section.Fields.Count);

            foreach (var field in section.Fields)
            {
                fields.Add(
                    new(
                        field.Path,
                        field.Label,
                        field.Type,
                        field.Required,
                        field.Help,
                        field.Options,
                        ReadChain(instance, field.Chain),
                        field.DefaultValue,
                        field.Secret
                    )
                );
            }

            sections.Add(new(section.Id, section.Label, fields));
        }

        return new(sections);
    }

    private static SectionSchema AddGeneralSection(List<SectionSchema> sections)
    {
        var general = new SectionSchema("general", "General", 0, []);
        sections.Add(general);

        return general;
    }

    private static FieldSchema BuildField(
        PropertyInfo property,
        ConfigFieldAttribute attr,
        string path,
        PropertyInfo[] chain,
        object? defaultInstance
    )
        => new(
            path,
            attr.Label,
            ResolveType(property, attr),
            attr.Required,
            attr.Help,
            attr.Options ?? [],
            attr.Secret,
            attr.Order,
            chain,
            ReadChain(defaultInstance, chain)
        );

    private static List<FieldSchema> BuildFields(
        Type type,
        string prefix,
        PropertyInfo[] parentChain,
        object? defaultInstance
    )
    {
        var fields = new List<FieldSchema>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var fieldAttr = property.GetCustomAttribute<ConfigFieldAttribute>();

            if (fieldAttr is null)
            {
                continue;
            }

            var path = $"{prefix}.{ConfigYamlOptions.ToConfigKey(property.Name)}";
            fields.Add(BuildField(property, fieldAttr, path, [.. parentChain, property], defaultInstance));
        }

        return fields;
    }

    private static FormSchema BuildSchema(Type type)
    {
        var defaultInstance = Activator.CreateInstance(type);
        var sections = new List<SectionSchema>();
        SectionSchema? general = null;

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var sectionAttr = property.GetCustomAttribute<ConfigSectionAttribute>();

            if (sectionAttr is not null)
            {
                if (!property.PropertyType.IsClass || property.PropertyType == typeof(string))
                {
                    throw new InvalidOperationException(
                        $"[ConfigSection] on '{type.Name}.{property.Name}' requires a class property."
                    );
                }

                var prefix = ConfigYamlOptions.ToConfigKey(property.Name);
                var fields = BuildFields(property.PropertyType, prefix, [property], defaultInstance);
                sections.Add(new(ToSectionId(sectionAttr.Label), sectionAttr.Label, sectionAttr.Order, fields));

                continue;
            }

            var fieldAttr = property.GetCustomAttribute<ConfigFieldAttribute>();

            if (fieldAttr is null)
            {
                continue;
            }

            general ??= AddGeneralSection(sections);
            general.Fields.Add(
                BuildField(property, fieldAttr, ConfigYamlOptions.ToConfigKey(property.Name), [property], defaultInstance)
            );
        }

        var orderedSections = sections
                              .Select((section, index) => (section, index))
                              .OrderBy(item => item.section.Order)
                              .ThenBy(item => item.index)
                              .Select(item => SortFields(item.section))
                              .ToList();

        return new(orderedSections);
    }

    private static bool IsNumeric(Type type)
        => type == typeof(int) ||
           type == typeof(long) ||
           type == typeof(short) ||
           type == typeof(byte) ||
           type == typeof(uint) ||
           type == typeof(ulong) ||
           type == typeof(ushort) ||
           type == typeof(sbyte) ||
           type == typeof(double) ||
           type == typeof(float) ||
           type == typeof(decimal);

    private static object? ReadChain(object? root, PropertyInfo[] chain)
    {
        var current = root;

        foreach (var property in chain)
        {
            if (current is null)
            {
                return null;
            }

            current = property.GetValue(current);
        }

        return current;
    }

    private static string ResolveType(PropertyInfo property, ConfigFieldAttribute attr)
    {
        if (attr.Type != ConfigFieldType.Auto)
        {
            return ToToken(attr.Type);
        }

        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (type == typeof(bool))
        {
            return PluginConfigFieldTypes.Boolean;
        }

        if (type == typeof(string))
        {
            return PluginConfigFieldTypes.Text;
        }

        if (IsNumeric(type))
        {
            return PluginConfigFieldTypes.Number;
        }

        throw new NotSupportedException(
            $"Cannot infer a config field type for '{property.DeclaringType?.Name}.{property.Name}' of type {type.Name}; set ConfigFieldAttribute.Type explicitly."
        );
    }

    private static SectionSchema SortFields(SectionSchema section)
    {
        var ordered = section.Fields
                             .Select((field, index) => (field, index))
                             .OrderBy(item => item.field.Order)
                             .ThenBy(item => item.index)
                             .Select(item => item.field)
                             .ToList();

        return section with { Fields = ordered };
    }

    private static string ToSectionId(string label)
        => label.Trim().ToLowerInvariant().Replace(' ', '_');

    private static string ToToken(ConfigFieldType type)
        => type switch
        {
            ConfigFieldType.Boolean  => PluginConfigFieldTypes.Boolean,
            ConfigFieldType.Number   => PluginConfigFieldTypes.Number,
            ConfigFieldType.Text     => PluginConfigFieldTypes.Text,
            ConfigFieldType.TextArea => PluginConfigFieldTypes.TextArea,
            _                        => PluginConfigFieldTypes.Text
        };
}
