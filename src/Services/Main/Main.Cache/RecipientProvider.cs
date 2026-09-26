using Application.Common.Interfaces.Cache;
using Application.Common.Interfaces.Repositories;
using Cache.Extensions;
using Main.Application.Interfaces.Cache;
using Main.Application.Static;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Notification.Core.Interfaces.Recipient;
using Notification.Core.Recipients;

namespace Main.Cache;

public class RecipientProvider(
	ICache cache,
	IReadRepository<UserNotificationPreference, UserNotificationPreferenceKey> preferenceRepository,
	IReadRepository<UserEmail, string> emailRepository
	) : IRecipientProvider
{
	public Task InvalidateUserRecipients(Guid userId) =>
		cache.RemoveKeyAsync(CacheKeys.UserCache.NotificationRecipients(userId));

	public Task InvalidateUsersRecipients(IEnumerable<Guid> userIds) =>
		cache.RemoveKeysAsync(userIds.Select(CacheKeys.UserCache.NotificationRecipients));

	public async Task<IReadOnlyCollection<INotificationRecipient>> ResolveAsync(
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		var recipients = await ResolveAsync(
			[userId],
			cancellationToken);
		return recipients.TryGetValue(userId, out var value) ? value : [];
	}

	public async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<INotificationRecipient>>> ResolveAsync(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default)
	{
		var cached = await ResolveCachedAsync(userIds, cancellationToken);
		var result = new Dictionary<Guid, IReadOnlyCollection<INotificationRecipient>>(cached.Count);

		foreach (var (userId, entry) in cached)
		{
			var recipients = CreateRecipients(userId, entry);
			if (recipients.Count != 0)
				result.Add(userId, recipients);
		}

		return result;
	}

	public async Task<IReadOnlyDictionary<Guid, TRecipient>> ResolveAsync<TRecipient>(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default
		) where TRecipient : INotificationRecipient
	{
		var recipientsByUser = await ResolveAsync(userIds, cancellationToken);
		var result = new Dictionary<Guid, TRecipient>(recipientsByUser.Count);

		foreach (var (userId, recipients) in recipientsByUser)
		{
			var recipient = recipients.OfType<TRecipient>().FirstOrDefault();
			if (recipient is not null)
				result.Add(userId, recipient);
		}

		return result;
	}

	private Task<Dictionary<Guid, CachedNotificationRecipients>> ResolveCachedAsync(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(userIds);
		cancellationToken.ThrowIfCancellationRequested();

		return cache.GetOrSetManyAsync(
			userIds,
			CacheKeys.UserCache.NotificationRecipients,
			entry => entry.UserId,
			missingIds => LoadMissingAsync(missingIds, cancellationToken),
			CacheKeys.UserCache.Ttl);
	}

	private async Task<Dictionary<Guid, CachedNotificationRecipients>> LoadMissingAsync(
		IReadOnlyCollection<Guid> missingIds,
		CancellationToken cancellationToken)
	{
		var preferences = await preferenceRepository.Query
			.AsNoTracking()
			.Where(preference => missingIds.Contains(preference.UserId) && preference.Enabled)
			.ToListAsync(cancellationToken);

		var emails = await emailRepository.Query
			.AsNoTracking()
			.Where(email => missingIds.Contains(email.UserId) && email.IsPrimary && email.Confirmed)
			.ToListAsync(cancellationToken);

		var channelsByUser = preferences
			.GroupBy(preference => preference.UserId)
			.ToDictionary(
				group => group.Key,
				group => group
					.Select(preference => preference.ChannelName)
					.ToList());

		var emailsByUser = emails
			.GroupBy(email => email.UserId)
			.ToDictionary(
				group => group.Key,
				group => group.First().Email.Value);

		var result = new Dictionary<Guid, CachedNotificationRecipients>(missingIds.Count);

		foreach (var userId in missingIds)
		{
			var channels = channelsByUser.GetValueOrDefault(userId) ?? [];

			if (!channels.Contains(InAppRecipient.ChannelName))
				channels.Add(InAppRecipient.ChannelName);

			result.Add(
				userId,
				new CachedNotificationRecipients(
					userId,
					channels,
					emailsByUser.GetValueOrDefault(userId)));
		}

		return result;
	}

	private static List<INotificationRecipient> CreateRecipients(
		Guid userId,
		CachedNotificationRecipients entry)
	{
		var result = new List<INotificationRecipient>(entry.Channels.Count);
		foreach (var channelName in entry.Channels)
			switch (channelName)
			{
				case InAppRecipient.ChannelName:
					result.Add(new InAppRecipient(userId));
					break;
				case EmailRecipient.ChannelName when entry.PrimaryEmail is not null:
					result.Add(new EmailRecipient(entry.PrimaryEmail));
					break;
			}

		return result;
	}

	private sealed record CachedNotificationRecipients(
		Guid UserId,
		List<string> Channels,
		string? PrimaryEmail);
}
