namespace Main.Application.Services;

internal static class NotificationCacheKeys
{
	public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);

	public static string NotificationRecipients(Guid userId) => $"user:{userId}:notification:recipients";
}
