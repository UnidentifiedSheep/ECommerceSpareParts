using Notification.Core.Interfaces.Notification;

namespace Main.Application.Notifications;

public class UserLoggedInNotification : INotification<UserLoggedInNotificationData>
{
	public string SystemName => "UserLoggedIn";
	public UserLoggedInNotificationData Model { get; }
}

public record UserLoggedInNotificationData : INotificationModelWithTitle
{
	public required string Title { get; init; }

}
