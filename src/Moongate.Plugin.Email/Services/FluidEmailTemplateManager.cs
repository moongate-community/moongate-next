using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using Fluid;
using Moongate.Plugin.Email.Data;
using Moongate.Plugin.Email.Interfaces;
using Serilog;

namespace Moongate.Plugin.Email.Services;

/// <summary>Liquid/Fluid file-system template manager for email messages.</summary>
public sealed class FluidEmailTemplateManager : IEmailTemplateManager
{
    private const string SubjectFileName = "subject.liquid";
    private const string TextFileName = "text.liquid";
    private const string HtmlFileName = "html.liquid";
    private static readonly FluidParser Parser = new();
    private readonly EmailPluginConfig _config;
    private readonly TemplateOptions _options;
    private readonly EmailPluginRuntimePaths _paths;

    private readonly ConcurrentDictionary<string, CachedTemplate> _templates = new(StringComparer.OrdinalIgnoreCase);

    public FluidEmailTemplateManager(EmailPluginConfig config, EmailPluginRuntimePaths paths)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(paths);

        _config = config;
        _paths = paths;
        _options = CreateOptions();

        EmailTemplateAssetsBootstrapper.EnsureDefaultTemplates(
            _config,
            _paths,
            Log.ForContext<FluidEmailTemplateManager>()
        );
    }

    internal string TemplatesRoot => EmailTemplateAssetsBootstrapper.ResolveTemplatesRoot(_config, _paths);

    public async ValueTask<RenderedEmailTemplate> RenderActivationAsync(
        string templateId,
        ActivationEmailModel model,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);
        ArgumentNullException.ThrowIfNull(model);

        var templateDirectory = GetTemplateDirectory(templateId);
        var subject = await RenderAsync(
            await LoadTemplateAsync(Path.Combine(templateDirectory, SubjectFileName), true, cancellationToken),
            model,
            false
        );
        var text = await RenderAsync(
            await LoadTemplateAsync(Path.Combine(templateDirectory, TextFileName), true, cancellationToken),
            model,
            false
        );
        var htmlTemplate = await LoadTemplateAsync(Path.Combine(templateDirectory, HtmlFileName), false, cancellationToken);
        var html = htmlTemplate is null ? null : await RenderAsync(htmlTemplate, model, true);

        return new RenderedEmailTemplate(subject.Trim(), text, string.IsNullOrWhiteSpace(html) ? null : html);
    }

    private static TemplateOptions CreateOptions()
    {
        var options = new TemplateOptions();
        options.MemberAccessStrategy.Register<ActivationEmailModel, string>("username", model => model.Username);
        options.MemberAccessStrategy.Register<ActivationEmailModel, string>("email", model => model.Email);
        options.MemberAccessStrategy.Register<ActivationEmailModel, string>("activation_id", model => model.ActivationId);
        options.MemberAccessStrategy.Register<ActivationEmailModel, string>(
            "activation_url",
            model => model.ActivationUrl
        );

        return options;
    }

    private string GetTemplateDirectory(string templateId)
    {
        return Path.Combine(TemplatesRoot, templateId);
    }

    private async ValueTask<IFluidTemplate?> LoadTemplateAsync(
        string path,
        bool required,
        CancellationToken cancellationToken
    )
    {
        if (!File.Exists(path))
        {
            if (required)
            {
                throw new InvalidOperationException($"Email template file '{path}' does not exist.");
            }

            return null;
        }

        var lastModified = File.GetLastWriteTimeUtc(path);

        if (!_config.Templates.ReloadOnChange &&
            _templates.TryGetValue(path, out var cachedWithoutReload))
        {
            return cachedWithoutReload.Template;
        }

        if (_templates.TryGetValue(path, out var cached) && cached.LastModified == lastModified)
        {
            return cached.Template;
        }

        var source = await File.ReadAllTextAsync(path, cancellationToken);

        if (!Parser.TryParse(source, out var template, out var error))
        {
            throw new InvalidOperationException($"Email template file '{path}' is invalid: {error}");
        }

        _templates[path] = new CachedTemplate(template, lastModified);

        return template;
    }

    private async ValueTask<string> RenderAsync(IFluidTemplate template, ActivationEmailModel model, bool htmlEncode)
    {
        var context = new TemplateContext(model, _options, false);

        return htmlEncode
            ? await template.RenderAsync(context, HtmlEncoder.Default)
            : await template.RenderAsync(context);
    }

    private sealed record CachedTemplate(IFluidTemplate Template, DateTime LastModified);
}
