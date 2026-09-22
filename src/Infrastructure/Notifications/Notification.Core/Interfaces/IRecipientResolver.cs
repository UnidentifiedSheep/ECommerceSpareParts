namespace Notification.Core.Interfaces;

public interface IRecipientResolver
{
	Task<IReadOnlyCollection<INotificationRecipient>> ResolveAsync(
		Guid userId,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<INotificationRecipient>>> ResolveAsync(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default);
}
