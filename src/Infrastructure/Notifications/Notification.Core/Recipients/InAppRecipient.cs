using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Recipient;

namespace Notification.Core.Recipients;

public sealed record InAppRecipient(Guid UserId) : INotificationRecipient
{
	public const string ChannelName = "InApp";
	static string INotificationRecipient.StaticChannelSystemName => ChannelName;

	public string ChannelSystemName => ChannelName;
}

public record InAppReceipt(
	InAppRecipient Recipient,
	int CreatedRowId) : IDeliveryReceipt<InAppRecipient>;
