using System.Globalization;
using System.Text.Json;
using DryIoc;
using Moongate.Abstractions.Configuration;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Services.Secrets;
using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;
using Moongate.Plugin.Email.Data;
using Moongate.Plugin.Email.Interfaces;
using Moongate.Plugin.Email.Services;
using Moongate.UO.Domain.Events;
using Serilog;

namespace Moongate.Plugin.Email;

/// <summary>Moongate email delivery plugin.</summary>
public sealed class EmailPlugin : IMoongatePlugin, IConfigurablePlugin, ITestablePlugin
{
    private const string BooleanField = "boolean";
    private const string NumberField = "number";
    private const string TextField = "text";
    private const string TextAreaField = "textarea";

    private readonly string? _defaultActivationBaseUrl;
    private readonly Func<EmailPluginConfig, IEmailConfigurationTester> _testerFactory;
    private EmailPluginConfig _config = new();
    private EmailPluginRuntimePaths? _paths;

    public EmailPlugin()
        : this((string?)null)
    {
    }

    public EmailPlugin(string? defaultActivationBaseUrl)
    {
        _defaultActivationBaseUrl = defaultActivationBaseUrl;
        _testerFactory = config => new SmtpEmailConfigurationTester(new EnvironmentSecretManagerService(config.Secrets));
    }

    internal EmailPlugin(Func<EmailPluginConfig, IEmailConfigurationTester> testerFactory)
    {
        ArgumentNullException.ThrowIfNull(testerFactory);

        _testerFactory = testerFactory;
    }

    public PluginMetadata Metadata { get; } = new()
    {
        Id = "moongate.email",
        Name = "Moongate Email Plugin",
        Version = new(0, 1, 0),
        Author = "Moongate",
        Description = "Sends account activation emails."
    };

    public void Configure(IContainer container, PluginContext context)
    {
        var config = context.LoadConfig(CreateDefaultConfig);
        var errors = config.Validate().ToArray();

        if (errors.Length > 0)
        {
            throw new InvalidOperationException($"Email plugin config is invalid: {string.Join("; ", errors)}");
        }

        var paths = new EmailPluginRuntimePaths(context.PluginDirectory);

        EmailTemplateAssetsBootstrapper.EnsureDefaultTemplates(config, paths, Log.ForContext<EmailPlugin>());

        _config = config;
        _paths = paths;

        container.RegisterInstance(config);
        container.RegisterInstance(paths);
        container.RegisterInstance<ISecretManagerService>(new EnvironmentSecretManagerService(config.Secrets));
        container.Register<IEmailTemplateManager, FluidEmailTemplateManager>(Reuse.Singleton);
        container.Register<IEmailSender, SmtpEmailSender>(Reuse.Singleton);
        container.AddAsyncEventHandler<UserActivationEmailHandler, UserCreatedEvent>();
    }

    public async ValueTask<PluginConfigForm> GetConfigFormAsync(CancellationToken cancellationToken = default)
    {
        var config = await LoadLatestConfigAsync(cancellationToken);

        return BuildConfigForm(config);
    }

    public async ValueTask<PluginConfigSaveResult> SaveConfigAsync(
        PluginConfigSaveRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var configPath = GetConfigPath();

        if (configPath is null)
        {
            return Failure("Email plugin has not been configured.");
        }

        if (request.Values is null)
        {
            return Failure("Config values are required.");
        }

        var config = await LoadLatestConfigAsync(cancellationToken);
        var errors = ApplyConfigValues(config, request.Values);

        errors.AddRange(config.Validate());

        if (errors.Count > 0)
        {
            return new(false, false, errors, null);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
        await File.WriteAllTextAsync(
            configPath,
            ConfigYamlOptions.Serializer.Serialize(config),
            cancellationToken
        );
        _config = config;

        return new(true, true, [], null);
    }

    public async ValueTask<PluginTestResult> TestAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await LoadLatestConfigAsync(cancellationToken);
            var errors = config.Validate().ToArray();

            if (errors.Length > 0)
            {
                return new(false, "Email plugin config is invalid.", errors);
            }

            return await _testerFactory(config).TestAsync(config, cancellationToken);
        }
        catch (Exception ex)
        {
            return new(false, "Email plugin test failed.", [ex.Message]);
        }
    }

    private static string? BuildDefaultActivationUrlTemplate(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        return $"{baseUrl.Trim().TrimEnd('/')}/activate?activation_id={{activation_id}}";
    }

    private EmailPluginConfig CreateDefaultConfig()
    {
        var config = new EmailPluginConfig();
        var activationUrlTemplate = BuildDefaultActivationUrlTemplate(_defaultActivationBaseUrl);

        if (!string.IsNullOrWhiteSpace(activationUrlTemplate))
        {
            config.Activation.UrlTemplate = activationUrlTemplate;
        }

        return config;
    }

    private static List<string> ApplyConfigValues(
        EmailPluginConfig config,
        IReadOnlyDictionary<string, object?> values
    )
    {
        var errors = new List<string>();

        foreach (var (path, value) in values)
        {
            switch (path)
            {
                case "enabled":
                    ApplyBool(value, path, errors, result => config.Enabled = result);
                    break;
                case "from.name":
                    config.From.Name = ReadString(value);
                    break;
                case "from.address":
                    config.From.Address = ReadString(value);
                    break;
                case "smtp.host":
                    config.Smtp.Host = ReadString(value);
                    break;
                case "smtp.port":
                    ApplyInt(value, path, errors, result => config.Smtp.Port = result);
                    break;
                case "smtp.username":
                    config.Smtp.Username = ReadString(value);
                    break;
                case "smtp.password_secret":
                    config.Smtp.PasswordSecret = ReadString(value);
                    break;
                case "smtp.use_ssl":
                    ApplyBool(value, path, errors, result => config.Smtp.UseSsl = result);
                    break;
                case "smtp.start_tls":
                    ApplyBool(value, path, errors, result => config.Smtp.StartTls = result);
                    break;
                case "smtp.timeout_seconds":
                    ApplyInt(value, path, errors, result => config.Smtp.TimeoutSeconds = result);
                    break;
                case "secrets.environment.prefix":
                    config.Secrets.Environment.Prefix = ReadString(value);
                    break;
                case "activation.template_id":
                    config.Activation.TemplateId = ReadString(value);
                    break;
                case "activation.url_template":
                    config.Activation.UrlTemplate = ReadString(value);
                    break;
                case "templates.directory":
                    config.Templates.Directory = ReadString(value);
                    break;
                case "templates.reload_on_change":
                    ApplyBool(value, path, errors, result => config.Templates.ReloadOnChange = result);
                    break;
                default:
                    errors.Add($"Unsupported config field '{path}'.");
                    break;
            }
        }

        return errors;
    }

    private static void ApplyBool(object? value, string path, List<string> errors, Action<bool> apply)
    {
        if (TryReadBool(value, out var result))
        {
            apply(result);

            return;
        }

        errors.Add($"Field '{path}' must be a boolean value.");
    }

    private static void ApplyInt(object? value, string path, List<string> errors, Action<int> apply)
    {
        if (TryReadInt(value, out var result))
        {
            apply(result);

            return;
        }

        errors.Add($"Field '{path}' must be a number value.");
    }

    private static PluginConfigForm BuildConfigForm(EmailPluginConfig config)
        => new(
            [
                new(
                    "general",
                    "General",
                    [
                        Field("enabled", "Enabled", BooleanField, config.Enabled, false)
                    ]
                ),
                new(
                    "sender",
                    "Sender",
                    [
                        Field("from.name", "From name", TextField, config.From.Name, true, defaultValue: "Moongate"),
                        Field("from.address", "From address", TextField, config.From.Address, true)
                    ]
                ),
                new(
                    "smtp",
                    "SMTP",
                    [
                        Field("smtp.host", "Host", TextField, config.Smtp.Host, true),
                        Field("smtp.port", "Port", NumberField, config.Smtp.Port, true, defaultValue: 587),
                        Field("smtp.username", "Username", TextField, config.Smtp.Username, true),
                        Field(
                            "smtp.password_secret",
                            "Password secret",
                            TextField,
                            config.Smtp.PasswordSecret,
                            true,
                            "Logical secret name resolved by the configured secret manager.",
                            "smtp_password",
                            true
                        ),
                        Field("smtp.use_ssl", "Use SSL", BooleanField, config.Smtp.UseSsl, false),
                        Field("smtp.start_tls", "STARTTLS", BooleanField, config.Smtp.StartTls, false),
                        Field("smtp.timeout_seconds", "Timeout seconds", NumberField, config.Smtp.TimeoutSeconds, true, defaultValue: 30)
                    ]
                ),
                new(
                    "activation",
                    "Activation",
                    [
                        Field("activation.template_id", "Template id", TextField, config.Activation.TemplateId, true, defaultValue: "account_activation"),
                        Field(
                            "activation.url_template",
                            "Activation URL template",
                            TextAreaField,
                            config.Activation.UrlTemplate,
                            true,
                            "Must contain {activation_id}."
                        )
                    ]
                ),
                new(
                    "templates",
                    "Templates",
                    [
                        Field("templates.directory", "Directory", TextField, config.Templates.Directory, true, defaultValue: "templates"),
                        Field("templates.reload_on_change", "Reload on change", BooleanField, config.Templates.ReloadOnChange, false)
                    ]
                )
            ]
        );

    private static PluginConfigField Field(
        string path,
        string label,
        string type,
        object? value,
        bool required,
        string? help = null,
        object? defaultValue = null,
        bool secretReference = false,
        IReadOnlyList<string>? options = null
    )
        => new(path, label, type, required, help, options ?? [], value, defaultValue, secretReference);

    private static PluginConfigSaveResult Failure(string error)
        => new(false, false, [error], null);

    private static string ReadString(object? value)
    {
        if (value is null)
        {
            return "";
        }

        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Null => "",
                JsonValueKind.String => element.GetString() ?? "",
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => element.ToString()
            };
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
    }

    private static bool TryReadBool(object? value, out bool result)
    {
        if (value is bool boolValue)
        {
            result = boolValue;

            return true;
        }

        if (value is JsonElement { ValueKind: JsonValueKind.True })
        {
            result = true;

            return true;
        }

        if (value is JsonElement { ValueKind: JsonValueKind.False })
        {
            result = false;

            return true;
        }

        return bool.TryParse(ReadString(value), out result);
    }

    private static bool TryReadInt(object? value, out int result)
    {
        if (value is int intValue)
        {
            result = intValue;

            return true;
        }

        if (value is JsonElement element && element.ValueKind == JsonValueKind.Number)
        {
            return element.TryGetInt32(out result);
        }

        return int.TryParse(ReadString(value), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    private string? GetConfigPath()
        => _paths is null ? null : Path.Combine(_paths.PluginDirectory, PluginContext.PluginConfigFileName);

    private async ValueTask<EmailPluginConfig> LoadLatestConfigAsync(CancellationToken cancellationToken)
    {
        var configPath = GetConfigPath();

        if (configPath is null || !File.Exists(configPath))
        {
            return _config;
        }

        var yaml = await File.ReadAllTextAsync(configPath, cancellationToken);

        return ConfigYamlOptions.Deserializer.Deserialize<EmailPluginConfig>(yaml) ?? new();
    }
}
