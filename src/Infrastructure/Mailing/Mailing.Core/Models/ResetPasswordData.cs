using System.Globalization;
using Locan.Core.Interfaces.Localizers;

namespace Mailing.Core.Models;

public sealed class ResetPasswordData : IEmailData
{
	public ResetPasswordData(
		IContextualLocalizer localizer,
		string resetUrl,
		string to)
	{
		Locale = CultureInfo.CurrentUICulture;
		To = to;
		ResetUrl = resetUrl;
		HtmlLang = Locale.ToString().ToLowerInvariant();
		Subject = localizer.Get(new MailPasswordResetSubjectMessage());
		Title = localizer.Get(new MailPasswordResetTitleMessage());
		Intro = localizer.Get(new MailPasswordResetIntroMessage());
		Description = localizer.Get(new MailPasswordResetDescriptionMessage());
		Button = localizer.Get(new MailPasswordResetButtonMessage());
		Fallback = localizer.Get(new MailPasswordResetFallbackMessage());
		Ignore = localizer.Get(new MailPasswordResetIgnoreMessage());
	}

	public CultureInfo Locale { get; }

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
