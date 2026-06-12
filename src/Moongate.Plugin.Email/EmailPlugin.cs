using DryIoc;
using Moongate.Abstractions.Configuration;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Services.Secrets;
using Moongate.Plugin.Email.Data;
using Moongate.Plugin.Email.Interfaces;
using Moongate.Plugin.Email.Services;
using Moongate.Plugins.Configuration;
using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;
using Moongate.UO.Domain.Events;
using Serilog;

namespace Moongate.Plugin.Email;

/// <summary>Moongate email delivery plugin.</summary>
public sealed class EmailPlugin : ConfigurablePlugin<EmailPluginConfig>, IMoongatePlugin, ITestablePlugin
{
    private readonly string? _defaultActivationBaseUrl;
    private readonly Func<EmailPluginConfig, IEmailConfigurationTester> _testerFactory;
    private EmailPluginConfig _config = new();
    private EmailPluginRuntimePaths? _paths;

    public EmailPlugin()
        : this((string?)null) { }

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

    protected override ValueTask<EmailPluginConfig> LoadConfigAsync(CancellationToken cancellationToken)
        => LoadLatestConfigAsync(cancellationToken);

    protected override async ValueTask<PluginConfigSaveResult> SaveTypedConfigAsync(
        EmailPluginConfig config,
        CancellationToken cancellationToken
    )
    {
        var configPath = GetConfigPath();

        if (configPath is null)
        {
            return Failure("Email plugin has not been configured.");
        }

        var errors = config.Validate().ToList();

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

    private static PluginConfigSaveResult Failure(string error)
        => new(false, false, [error], null);

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
