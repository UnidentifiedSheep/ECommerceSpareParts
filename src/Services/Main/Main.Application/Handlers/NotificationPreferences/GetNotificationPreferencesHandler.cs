using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Locan.Core.Interfaces.Localizers;
using Locan.Core.LocalizableMessages;
using Main.Application.Dtos.NotificationPreference;
using Main.Application.Dtos.Users;
using Main.Entities;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Notification.Core.Recipients;

namespace Main.Application.Handlers.NotificationPreferences;

public record GetNotificationPreferencesQuery(Guid UserId) : IQuery<GetNotificationPreferencesResult>;

public record GetNotificationPreferencesResult(
	IReadOnlyList<UserNotificationPreferenceDto> Preferences);

public class GetNotificationPreferencesHandler(
	IReadRepository<UserNotificationPreference, UserNotificationPreferenceKey> repository,
	IProjectionProvider<UserNotificationPreference, UserNotificationPreferenceDto> projection,
	IContextualLocalizer localizer
	) : IQueryHandler<GetNotificationPreferencesQuery, GetNotificationPreferencesResult>
{
	public async Task<GetNotificationPreferencesResult> Handle(
		GetNotificationPreferencesQuery request,
		CancellationToken cancellationToken)
	{
		var userPreferences = await repository.Query
			.Where(x => x.UserId == request.UserId)
			.Project(projection)
			.ToListAsync(cancellationToken);

		if (userPreferences.All(x => x.ChannelName != InAppRecipient.ChannelName))
			userPreferences.Add(new UserNotificationPreferenceDto
			{
				ChannelName = InAppRecipient.ChannelName,
				Enabled = true,
				LocalizableChannelName = localizer.Get(InAppChannelMessage.Instance),
				UserId = request.UserId
			});

		if (userPreferences.Count == Recipients.All.Count)
			return new GetNotificationPreferencesResult(userPreferences);

		userPreferences.AddRange(Recipients
			.All
			.Keys
			.Where(channelName => userPreferences.All(x => x.ChannelName != channelName))
			.Select(channelToAdd => new UserNotificationPreferenceDto
			{
				ChannelName = channelToAdd,
				Enabled = false,
				UserId = request.UserId,
				LocalizableChannelName = localizer.Get(new LocalizableMessage($"{channelToAdd}Channel"))
			}));

		return new GetNotificationPreferencesResult(userPreferences);
	}
}
