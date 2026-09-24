using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Recipient;

namespace Notification.Core.Recipients;

public sealed record EmailRecipient(
	string Email,
	string Subject) : INotificationRecipient
{
	public const string ChannelName = "Email";
	public string ChannelSystemName => ChannelName;
}

public record EmailReceipt(EmailRecipient Recipient) : IDeliveryReceipt<EmailRecipient>;
