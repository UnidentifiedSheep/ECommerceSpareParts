namespace Notification.Core.Interfaces.Recipient;

public interface IRecipientResolver
{
	Task<IReadOnlyCollection<INotificationRecipient>> ResolveAsync(
		Guid userId,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<INotificationRecipient>>> ResolveAsync(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<TRecipient>>> ResolveAsync<TRecipient>(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default
		) where TRecipient : INotificationRecipient;
}
