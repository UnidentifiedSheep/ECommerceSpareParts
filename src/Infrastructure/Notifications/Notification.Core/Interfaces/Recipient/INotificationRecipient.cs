using System.Text.Json.Serialization;
using Notification.Core.Recipients;

namespace Notification.Core.Interfaces.Recipient;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$recipient")]
[JsonDerivedType(typeof(EmailRecipient), EmailRecipient.ChannelName)]
[JsonDerivedType(typeof(InAppRecipient), InAppRecipient.ChannelName)]
public interface INotificationRecipient
{
	static virtual string StaticChannelSystemName => throw new NotSupportedException(
		"The notification recipient must define a channel name.");

	string ChannelSystemName { get; }
}
