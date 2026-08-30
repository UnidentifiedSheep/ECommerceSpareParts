using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;

namespace Mailing.Core.Models;

public sealed class EmailVerificationData : IEmailData
{
	public EmailVerificationData(
		IContextualStringLocalizer localizer,
		string verificationUrl,
		string to)
	{
		Locale = localizer.Locale;
		To = to;
		VerificationUrl = verificationUrl;
		HtmlLang = Locale.ToString().ToLowerInvariant();
		Subject = localizer.Get("mail.email.verification.subject");
		Title = localizer.Get("mail.email.verification.title");
		Intro = localizer.Get("mail.email.verification.intro");
		Description = localizer.Get("mail.email.verification.description");
		Button = localizer.Get("mail.email.verification.button");
		Fallback = localizer.Get("mail.email.verification.fallback");
		Ignore = localizer.Get("mail.email.verification.ignore");
	}

	public Locale Locale { get; }

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
