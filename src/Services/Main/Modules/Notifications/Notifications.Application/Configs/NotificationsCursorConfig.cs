using Application.Common.Querying;
using Main.Entities.Notification;

namespace Main.Modules.Notifications.Application.Configs;

public static class NotificationsCursorConfig
{
	public static void Configure()
	{
		QueryableCursor.Value.Map<InAppNotification, DateTime>(x => x.CreateAt, true);
	}
}
