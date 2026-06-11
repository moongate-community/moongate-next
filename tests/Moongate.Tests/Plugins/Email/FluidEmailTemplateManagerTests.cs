using Moongate.Plugin.Email.Data;
using Moongate.Plugin.Email.Services;

namespace Moongate.Tests.Plugins.Email;

public sealed class FluidEmailTemplateManagerTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"moongate-email-templates-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_CreatesDefaultActivationTemplates()
    {
        var manager = CreateManager();
        var templateDirectory = Path.Combine(manager.TemplatesRoot, "account_activation");

        Assert.True(File.Exists(Path.Combine(templateDirectory, "subject.liquid")));
        Assert.True(File.Exists(Path.Combine(templateDirectory, "text.liquid")));
        Assert.True(File.Exists(Path.Combine(templateDirectory, "html.liquid")));
    }

    [Fact]
    public async Task RenderActivationAsync_DefaultTemplate_RendersTextAndHtml()
    {
        var manager = CreateManager();
        var model = new ActivationEmailModel(
            "<Squid>",
            "squid@example.com",
            "token",
            "https://example.com/activate?activation_id=token"
        );

        var rendered = await manager.RenderActivationAsync("account_activation", model);

        Assert.Equal("Activate your Moongate account", rendered.Subject);
        Assert.Contains("Hi <Squid>", rendered.TextBody);
        Assert.Contains("https://example.com/activate?activation_id=token", rendered.TextBody);
        Assert.NotNull(rendered.HtmlBody);
        Assert.Contains("Hi &lt;Squid&gt;", rendered.HtmlBody);
    }

    [Fact]
    public async Task RenderActivationAsync_ReloadsChangedTemplateWhenEnabled()
    {
        var manager = CreateManager();
        var model = new ActivationEmailModel("Squid", "squid@example.com", "token", "https://example.com/a/token");
        var subjectPath = Path.Combine(manager.TemplatesRoot, "account_activation", "subject.liquid");

        var first = await manager.RenderActivationAsync("account_activation", model);
        File.WriteAllText(subjectPath, "New subject for {{ username }}\n");
        File.SetLastWriteTimeUtc(subjectPath, DateTime.UtcNow.AddMinutes(1));

        var second = await manager.RenderActivationAsync("account_activation", model);

        Assert.Equal("Activate your Moongate account", first.Subject);
        Assert.Equal("New subject for Squid", second.Subject);
    }

    [Fact]
    public async Task RenderActivationAsync_MissingRequiredTemplate_Throws()
    {
        var manager = CreateManager();
        File.Delete(Path.Combine(manager.TemplatesRoot, "account_activation", "subject.liquid"));
        var model = new ActivationEmailModel("Squid", "squid@example.com", "token", "https://example.com/a/token");

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await manager.RenderActivationAsync("account_activation", model)
        );
    }

    private FluidEmailTemplateManager CreateManager()
        => new(new EmailPluginConfig(), new(_root));
}
