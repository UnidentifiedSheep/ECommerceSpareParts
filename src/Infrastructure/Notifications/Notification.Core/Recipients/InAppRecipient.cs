using Notification.Core.Interfaces;

namespace Notification.Core.Recipients;

public sealed record InAppRecipient(Guid UserId) : INotificationRecipient
{
	public const string ChannelName = "InApp";

	public string ChannelSystemName => ChannelName;
}
