using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Locan.Core.Interfaces.Localizers;
using Main.Entities;
using Notification.Core.Interfaces.Notification;

namespace Main.Application.Notifications;

public class PasswordResetNotification : INotification<PasswordResetNotificationData>
{
	public PasswordResetNotification(PasswordResetNotificationData model) => Model = model;

	public string SystemName => "PasswordReset";
	public PasswordResetNotificationData Model { get; }
}

public record PasswordResetNotificationData : INotificationModel, INotificationModelWithTitle
{
	public required string HtmlLang { get; init; }
	public required string Title { get; init; }
	public required string Heading { get; init; }
	public required string Intro { get; init; }
	public required string Description { get; init; }
	public required string Button { get; init; }
	public required string Fallback { get; init; }
	public required string Ignore { get; init; }
	public required string ResetUrl { get; init; }
	public required string AsText { get; init; }

	public PasswordResetNotificationData() { }

	[SetsRequiredMembers]
	public PasswordResetNotificationData(
		IContextualLocalizer localizer,
		string resetUrl)
	{
		HtmlLang = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
		Title = localizer.Get(NotificationsPasswordResetSubjectMessage.Instance);
		Heading = localizer.Get(NotificationsPasswordResetTitleMessage.Instance);
		Intro = localizer.Get(NotificationsPasswordResetIntroMessage.Instance);
		Description = localizer.Get(NotificationsPasswordResetDescriptionMessage.Instance);
		Button = localizer.Get(NotificationsPasswordResetButtonMessage.Instance);
		Fallback = localizer.Get(NotificationsPasswordResetFallbackMessage.Instance);
		Ignore = localizer.Get(NotificationsPasswordResetIgnoreMessage.Instance);
		ResetUrl = resetUrl;
		AsText = $"{Heading}. {Intro} {Description} {Fallback} {ResetUrl} {Ignore}";
	}
}
