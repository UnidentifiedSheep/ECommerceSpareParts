using Application.Common.Interfaces.Repositories;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Notification.Core.Interfaces.Recipient;
using Notification.Core.Recipients;

namespace Main.Application.Services.Notification;

public class RecipientResolver(
	IReadRepository<UserNotificationPreference, UserNotificationPreferenceKey> readRepository,
	IReadRepository<UserEmail, string> emailRepository
	) : IRecipientResolver
{
	public async Task<IReadOnlyCollection<INotificationRecipient>> ResolveAsync(
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		var preferences = await readRepository.Query
			.Where(x => x.UserId == userId)
			.ToListAsync(cancellationToken);

		var emails = (await emailRepository.Query
			.Where(x => x.UserId == userId)
			.Where(x => x.IsPrimary && x.Confirmed)
			.ToListAsync(cancellationToken))
			.GroupBy(x => x.UserId)
			.ToDictionary(x => x.Key, x => x.First().Email.Value);

		var result = new List<INotificationRecipient>();

		foreach (var preference in preferences)
		{
			var recipient = CreateRecipient(preference, emails);
			if (recipient == null) continue;
			result.Add(recipient);
		}

		return result;
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

	private static INotificationRecipient? CreateRecipient(
		UserNotificationPreference notificationPreference,
		IReadOnlyDictionary<Guid, string> emails)
		=> notificationPreference.ChannelName switch
		{
			InAppRecipient.ChannelName => new InAppRecipient(notificationPreference.UserId),
			EmailRecipient.ChannelName => emails.TryGetValue(notificationPreference.UserId, out var email)
				? new EmailRecipient(email)
				: null,
			_ => null
		};
}
