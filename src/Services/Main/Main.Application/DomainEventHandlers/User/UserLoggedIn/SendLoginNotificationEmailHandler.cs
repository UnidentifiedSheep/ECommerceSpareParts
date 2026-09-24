using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Notifications;
using Main.Entities.DomainEvents.User;
using Notification.Core;
using Notification.Core.Interfaces.Notification;

namespace Main.Application.DomainEventHandlers.User.UserLoggedIn;

public class SendLoginNotificationEmailHandler(
	INotificationService notificationService,
	IContextualLocalizer localizer) : BatchableDomainEventHandler<UserLoggedInDomainEvent>
{
	public override async Task Handle(
		Batch<UserLoggedInDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var events = notification.Items;
		if (events.Count == 0) return;

		var notifications = events
			.Select(@event => new NotificationItem(
				@event.UserId,
				new UserLoggedInNotification(
					new UserLoggedInNotificationData(
						localizer,
						@event.OccurredAtUtc,
						@event.IpAddress,
						@event.UserAgent))))
			.ToList();

		await notificationService.QueueAsync(notifications, cancellationToken);
	}
}
