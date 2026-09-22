using Notification.Core.Interfaces;

namespace Notification.Core.Recipients;

public record InAppReceipt(
	InAppRecipient Recipient,
	int CreatedRowId) : IDeliveryReceipt<InAppRecipient>;
