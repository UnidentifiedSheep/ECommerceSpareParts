namespace Notification.Core.Interfaces.Recipient;

public interface INotificationRecipient
{
	static virtual string StaticChannelSystemName => throw new NotSupportedException(
		"The notification recipient must define a channel name.");

	string ChannelSystemName { get; }
}
