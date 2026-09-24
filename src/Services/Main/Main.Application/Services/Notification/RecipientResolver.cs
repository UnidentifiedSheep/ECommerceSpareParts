using Notification.Core.Interfaces.Recipient;

namespace Main.Application.Services.Notification;

public class RecipientResolver : IRecipientResolver
{
	public Task<IReadOnlyCollection<INotificationRecipient>> ResolveAsync(
		Guid userId,
		CancellationToken cancellationToken = default)
	{

	}

	public Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<INotificationRecipient>>> ResolveAsync(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default)
	{

	}

	public Task<IReadOnlyDictionary<Guid, TRecipient>> ResolveAsync<TRecipient>(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default
		) where TRecipient : INotificationRecipient
	{

	}
}
