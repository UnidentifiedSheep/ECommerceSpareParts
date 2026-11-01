using Abstractions;
using Application.Common.Querying;
using Main.Application.Analytics.Configs;
using Notification.Core.Entities;

namespace Main.Application.Configs;

public static class CursorConfig
{
	public static void Configure()
	{
		AnalyticsCursorConfig.Configure();
		QueryableCursor.Value.Map<InAppNotification, DateTime>(x => x.CreateAt, true);
	}
}
