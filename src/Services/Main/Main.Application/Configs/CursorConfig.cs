using Abstractions;
using Notification.Core.Entities;

namespace Main.Application.Configs;

public static class CursorConfig
{
	public static void Configure()
	{
		QueryableCursor.Value.Map<InAppNotification, DateTime>(x => x.CreateAt, true);
	}
}
