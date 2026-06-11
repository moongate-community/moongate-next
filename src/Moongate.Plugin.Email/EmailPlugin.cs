using DryIoc;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Services.Secrets;
using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;
using Moongate.Plugin.Email.Data;
using Moongate.Plugin.Email.Interfaces;
using Moongate.Plugin.Email.Services;
using Moongate.UO.Domain.Events;

namespace Moongate.Plugin.Email;

/// <summary>Moongate email delivery plugin.</summary>
public sealed class EmailPlugin : IMoongatePlugin
{
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
        var config = context.LoadConfig(() => new EmailPluginConfig());
        var errors = config.Validate().ToArray();

        if (errors.Length > 0)
        {
            throw new InvalidOperationException($"Email plugin config is invalid: {string.Join("; ", errors)}");
        }

        container.RegisterInstance(config);
        container.RegisterInstance(new EmailPluginRuntimePaths(context.PluginDirectory));
        container.RegisterInstance<ISecretManagerService>(new EnvironmentSecretManagerService(config.Secrets));
        container.Register<IEmailTemplateManager, FluidEmailTemplateManager>(Reuse.Singleton);
        container.Register<IEmailSender, SmtpEmailSender>(Reuse.Singleton);
        container.AddAsyncEventHandler<UserActivationEmailHandler, UserCreatedEvent>();
    }
}
