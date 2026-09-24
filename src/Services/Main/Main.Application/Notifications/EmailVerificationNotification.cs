using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Locan.Core.Interfaces.Localizers;
using Main.Entities;
using Notification.Core.Interfaces.Notification;

namespace Main.Application.Notifications;

public class EmailVerificationNotification : INotification<EmailVerificationNotificationData>
{
	public EmailVerificationNotification(EmailVerificationNotificationData model) => Model = model;

	public string SystemName => "EmailVerification";
	public EmailVerificationNotificationData Model { get; }
}

public record EmailVerificationNotificationData : INotificationModel, INotificationModelWithTitle
{
	public required string HtmlLang { get; init; }
	public required string Title { get; init; }
	public required string Heading { get; init; }
	public required string Intro { get; init; }
	public required string Description { get; init; }
	public required string Button { get; init; }
	public required string Fallback { get; init; }
	public required string Ignore { get; init; }
	public required string VerificationUrl { get; init; }
	public required string AsText { get; init; }

	public EmailVerificationNotificationData() { }

	[SetsRequiredMembers]
	public EmailVerificationNotificationData(
		IContextualLocalizer localizer,
		string verificationUrl)
	{
		HtmlLang = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
		Title = localizer.Get(NotificationsEmailVerificationSubjectMessage.Instance);
		Heading = localizer.Get(NotificationsEmailVerificationTitleMessage.Instance);
		Intro = localizer.Get(NotificationsEmailVerificationIntroMessage.Instance);
		Description = localizer.Get(NotificationsEmailVerificationDescriptionMessage.Instance);
		Button = localizer.Get(NotificationsEmailVerificationButtonMessage.Instance);
		Fallback = localizer.Get(NotificationsEmailVerificationFallbackMessage.Instance);
		Ignore = localizer.Get(NotificationsEmailVerificationIgnoreMessage.Instance);
		VerificationUrl = verificationUrl;
		AsText = $"{Heading}. {Intro} {Description} {Fallback} {VerificationUrl} {Ignore}";
	}
}
