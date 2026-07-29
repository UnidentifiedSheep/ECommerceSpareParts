using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;

namespace Mailing.Core.Models;

public sealed class ResetPasswordData : IEmailData
{
    public ResetPasswordData(
        IScopedStringLocalizer localizer,
        string resetUrl,
        string to)
    {
        Locale = localizer.Locale;
        To = to;
        ResetUrl = resetUrl;
        HtmlLang = Locale.ToString().ToLowerInvariant();
        Subject = localizer.Get("mail.password.reset.subject");
        Title = localizer.Get("mail.password.reset.title");
        Intro = localizer.Get("mail.password.reset.intro");
        Description = localizer.Get("mail.password.reset.description");
        Button = localizer.Get("mail.password.reset.button");
        Fallback = localizer.Get("mail.password.reset.fallback");
        Ignore = localizer.Get("mail.password.reset.ignore");
    }

    public Locale Locale { get; }
    public string HtmlLang { get; }
    public string Title { get; }
    public string Intro { get; }
    public string Description { get; }
    public string Button { get; }
    public string Fallback { get; }
    public string Ignore { get; }
    public string ResetUrl { get; }
    public string TemplateName => "PasswordReset";

    public string Subject { get; }
    public string To { get; }
}
