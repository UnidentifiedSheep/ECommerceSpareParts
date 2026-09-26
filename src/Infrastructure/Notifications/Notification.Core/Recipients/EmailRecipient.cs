using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Recipient;

namespace Notification.Core.Recipients;

public sealed record EmailRecipient(string Email) : INotificationRecipient
{
	public const string ChannelName = "Email";
	static string INotificationRecipient.StaticChannelSystemName => ChannelName;
	public string ChannelSystemName => ChannelName;
}

public record EmailReceipt(EmailRecipient Recipient) : IDeliveryReceipt<EmailRecipient>;
