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
		Subject = localizer.Get(new MailEmailVerificationSubjectMessage());
		Title = localizer.Get(new MailEmailVerificationTitleMessage());
		Intro = localizer.Get(new MailEmailVerificationIntroMessage());
		Description = localizer.Get(new MailEmailVerificationDescriptionMessage());
		Button = localizer.Get(new MailEmailVerificationButtonMessage());
		Fallback = localizer.Get(new MailEmailVerificationFallbackMessage());
		Ignore = localizer.Get(new MailEmailVerificationIgnoreMessage());
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
