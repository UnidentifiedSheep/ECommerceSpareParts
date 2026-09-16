using System.Globalization;
using Locan.Core.Interfaces.Localizers;

namespace Mailing.Core.Models;

public sealed class EmailVerificationData : IEmailData
{
	public EmailVerificationData(
		IContextualLocalizer localizer,
		string verificationUrl,
		string to)
	{
		Locale = CultureInfo.CurrentUICulture;
		To = to;
		VerificationUrl = verificationUrl;
		HtmlLang = Locale.ToString().ToLowerInvariant();
		Subject = localizer.Get(MailEmailVerificationSubjectMessage.Instance);
		Title = localizer.Get(MailEmailVerificationTitleMessage.Instance);
		Intro = localizer.Get(MailEmailVerificationIntroMessage.Instance);
		Description = localizer.Get(MailEmailVerificationDescriptionMessage.Instance);
		Button = localizer.Get(MailEmailVerificationButtonMessage.Instance);
		Fallback = localizer.Get(MailEmailVerificationFallbackMessage.Instance);
		Ignore = localizer.Get(MailEmailVerificationIgnoreMessage.Instance);
	}

	public CultureInfo Locale { get; }

	public string HtmlLang { get; }

	public string Title { get; }

	public string Intro { get; }

	public string Description { get; }

	public string Button { get; }

	public string Fallback { get; }

	public string Ignore { get; }

	public string VerificationUrl { get; }

	public string TemplateName => "EmailVerification";

	public string Subject { get; }

	public string To { get; }
}
