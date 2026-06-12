using Moongate.Plugins.Configuration;
using Moongate.Plugins.Data;
using Moongate.Plugins.Types;

namespace Moongate.Tests.Plugins.Configuration;

public class ConfigFormScannerTests
{
    [Fact]
    public void BuildForm_AutoOnUnsupportedType_Throws()
        => Assert.Throws<NotSupportedException>(() => ConfigFormScanner.BuildForm(new FormUnsupportedSample()));

    [Fact]
    public void BuildForm_DerivesNestedPaths()
    {
        var form = ConfigFormScanner.BuildForm(new FormSample());

        var paths = AllFields(form).Select(field => field.Path).ToArray();
        Assert.Contains("smtp.host", paths);
        Assert.Contains("smtp.port", paths);
        Assert.Contains("smtp.password_secret", paths);
        Assert.Contains("smtp.use_ssl", paths);
    }

    [Fact]
    public void BuildForm_ExcludesUnannotatedProperties()
    {
        var form = ConfigFormScanner.BuildForm(new FormSample());

        Assert.DoesNotContain(AllFields(form), field => field.Path.StartsWith("secrets.", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildForm_FieldMetadata_AndCurrentValue()
    {
        var instance = new FormSample();
        instance.Smtp.Port = 2525;

        var form = ConfigFormScanner.BuildForm(instance);

        var secret = Find(form, "smtp.password_secret");
        Assert.True(secret.Required);
        Assert.True(secret.SecretReference);
        Assert.Equal("h", secret.Help);

        var port = Find(form, "smtp.port");
        Assert.Equal(587, port.DefaultValue);
        Assert.Equal(2525, port.Value);
    }

    [Fact]
    public void BuildForm_InfersTypes_AndHonorsOverride()
    {
        var form = ConfigFormScanner.BuildForm(new FormSample());

        Assert.Equal(PluginConfigFieldTypes.Number, Find(form, "smtp.port").Type);
        Assert.Equal(PluginConfigFieldTypes.Text, Find(form, "smtp.host").Type);
        Assert.Equal(PluginConfigFieldTypes.Boolean, Find(form, "smtp.use_ssl").Type);
        Assert.Equal(PluginConfigFieldTypes.TextArea, Find(form, "smtp.notes").Type);
    }

    [Fact]
    public void BuildForm_SectionsInDeclarationOrder_WithGeneralFirst()
    {
        var form = ConfigFormScanner.BuildForm(new FormSample());

        Assert.Equal(["general", "sender", "smtp"], form.Sections.Select(section => section.Id).ToArray());
        Assert.Equal("General", form.Sections[0].Label);
        Assert.Equal("SMTP", form.Sections[2].Label);
    }

    [Fact]
    public void BuildForm_TopLevelField_LandsInGeneral()
    {
        var form = ConfigFormScanner.BuildForm(new FormSample());

        var general = form.Sections.Single(section => section.Id == "general");
        var field = Assert.Single(general.Fields);
        Assert.Equal("enabled", field.Path);
        Assert.Equal(PluginConfigFieldTypes.Boolean, field.Type);
    }

    private static IEnumerable<PluginConfigField> AllFields(PluginConfigForm form)
        => form.Sections.SelectMany(section => section.Fields);

    private static PluginConfigField Find(PluginConfigForm form, string path)
        => AllFields(form).Single(field => field.Path == path);
}

public sealed class FormSample
{
    [ConfigField("Enabled")]
    public bool Enabled { get; set; }

    [ConfigSection("Sender")]
    public FormSenderSample Sender { get; set; } = new();

    [ConfigSection("SMTP")]
    public FormSmtpSample Smtp { get; set; } = new();

    public FormSecretSample Secrets { get; set; } = new();
}

public sealed class FormSenderSample
{
    [ConfigField("From name")]
    public string Name { get; set; } = "Moongate";

    [ConfigField("From address", Required = true)]
    public string Address { get; set; } = "";
}

public sealed class FormSmtpSample
{
    [ConfigField("Host", Required = true)]
    public string Host { get; set; } = "";

    [ConfigField("Port", Required = true)]
    public int Port { get; set; } = 587;

    [ConfigField("Password secret", Required = true, Secret = true, Help = "h")]
    public string PasswordSecret { get; set; } = "smtp_password";

    [ConfigField("Use SSL")]
    public bool UseSsl { get; set; }

    [ConfigField("Notes", Type = ConfigFieldType.TextArea)]
    public string Notes { get; set; } = "";
}

public sealed class FormSecretSample
{
    [ConfigField("Prefix")]
    public string Prefix { get; set; } = "X";
}

public sealed class FormUnsupportedSample
{
    [ConfigField("When")]
    public DateTime When { get; set; }
}
