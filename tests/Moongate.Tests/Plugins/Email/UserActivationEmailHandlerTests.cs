using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Plugin.Email.Data;
using Moongate.Plugin.Email.Interfaces;
using Moongate.Plugin.Email.Services;
using Moongate.UO.Domain.Events;

namespace Moongate.Tests.Plugins.Email;

public sealed class UserActivationEmailHandlerTests
{
    private sealed class CapturingEmailSender : IEmailSender
    {
        public List<EmailMessage> Messages { get; } = [];

        public ValueTask SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);

            return ValueTask.CompletedTask;
        }
    }

    private sealed class CapturingTemplateManager : IEmailTemplateManager
    {
        public ActivationEmailModel? Model { get; private set; }

        public ValueTask<RenderedEmailTemplate> RenderActivationAsync(
            string templateId,
            ActivationEmailModel model,
            CancellationToken cancellationToken = default
        )
        {
            Model = model;

            return ValueTask.FromResult(new RenderedEmailTemplate("Subject", "Text", "<p>Html</p>"));
        }
    }

    [Fact]
    public async Task HandleAsync_PendingUser_SendsActivationEmail()
    {
        var config = EnabledConfig();
        var templates = new CapturingTemplateManager();
        var sender = new CapturingEmailSender();
        var handler = new UserActivationEmailHandler(config, templates, sender);
        var evt = Created(isActive: false, email: "squid@example.com", activationId: "token value");

        await handler.HandleAsync(evt, CancellationToken.None);

        var message = Assert.Single(sender.Messages);
        Assert.Equal("Squid", message.ToName);
        Assert.Equal("squid@example.com", message.ToAddress);
        Assert.Equal("Subject", message.Subject);
        Assert.NotNull(templates.Model);
        Assert.Equal("token value", templates.Model!.ActivationId);
        Assert.Equal("https://example.com/activate?activation_id=token%20value", templates.Model.ActivationUrl);
    }

    [Fact]
    public async Task HandleAsync_SenderThrows_SwallowsFailure()
    {
        var handler = new UserActivationEmailHandler(
            EnabledConfig(),
            new CapturingTemplateManager(),
            new ThrowingEmailSender()
        );

        await handler.HandleAsync(Created(isActive: false, email: "squid@example.com", activationId: "token"), CancellationToken.None);
    }

    [Theory]
    [InlineData(false, true, "squid@example.com", "token")]
    [InlineData(true, true, "squid@example.com", "token")]
    [InlineData(true, false, "", "token")]
    [InlineData(true, false, "squid@example.com", "")]
    public async Task HandleAsync_WhenNotEligible_DoesNotSend(
        bool enabled,
        bool isActive,
        string email,
        string activationId
    )
    {
        var config = EnabledConfig();
        config.Enabled = enabled;
        var sender = new CapturingEmailSender();
        var handler = new UserActivationEmailHandler(config, new CapturingTemplateManager(), sender);

        await handler.HandleAsync(Created(isActive, email, activationId), CancellationToken.None);

        Assert.Empty(sender.Messages);
    }

    private static UserCreatedEvent Created(bool isActive, string? email, string? activationId)
        => new(new Serial(1), "Squid", UserLevelType.Player, isActive, DateTimeOffset.UtcNow, email, activationId);

    private static EmailPluginConfig EnabledConfig()
        => new()
        {
            Enabled = true,
            Activation =
            {
                UrlTemplate = "https://example.com/activate?activation_id={activation_id}"
            }
        };

    private sealed class ThrowingEmailSender : IEmailSender
    {
        public ValueTask SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("SMTP failed");
    }
}
