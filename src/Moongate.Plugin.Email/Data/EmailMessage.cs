namespace Moongate.Plugin.Email.Data;

/// <summary>Fully rendered outbound email message.</summary>
public sealed record EmailMessage(
    string ToName,
    string ToAddress,
    string Subject,
    string TextBody,
    string? HtmlBody
);
