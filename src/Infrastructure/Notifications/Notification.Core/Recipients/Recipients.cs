namespace Notification.Core.Recipients;

public static class Recipients
{
	public static IReadOnlyDictionary<string, Type> All = new Dictionary<string, Type>
	{
		[InAppRecipient.ChannelName] = typeof(InAppRecipient),
		[EmailRecipient.ChannelName] = typeof(EmailRecipient)
	};
}
