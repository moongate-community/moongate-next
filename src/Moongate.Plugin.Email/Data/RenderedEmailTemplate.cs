namespace Moongate.Plugin.Email.Data;

/// <summary>Rendered subject and body content for an email template.</summary>
public sealed record RenderedEmailTemplate(
    string Subject,
    string TextBody,
    string? HtmlBody
);
