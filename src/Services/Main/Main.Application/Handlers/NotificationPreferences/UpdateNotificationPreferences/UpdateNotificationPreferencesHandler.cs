using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.NotificationPreference;
using Main.Entities.User;
using MediatR;
using Notification.Core.Recipients;

namespace Main.Application.Handlers.NotificationPreferences.UpdateNotificationPreferences;

[Transactional, AutoSave]
public record UpdateNotificationPreferencesCommand(
	Guid UserId,
	IReadOnlyCollection<UserNotificationPreferencePatchDto> Preferences) : ICommand;

public class UpdateNotificationPreferencesHandler(
	IRepository<UserNotificationPreference, UserNotificationPreferenceKey> repository,
	IUnitOfWork unitOfWork
	) : ICommandHandler<UpdateNotificationPreferencesCommand>
{
	public async Task<Unit> Handle(
		UpdateNotificationPreferencesCommand request,
		CancellationToken cancellationToken)
	{
		if (request.Preferences.Count == 0) return Unit.Value;
		var channels = request.Preferences.Select(x => x.ChannelName).ToList();

		var preferences = (await repository.ListAsync(
			Criteria<UserNotificationPreference>
				.New()
				.Where(x => channels.Contains(x.ChannelName))
				.Track()
				.Build(),
			cancellationToken))
			.ToDictionary(x => x.ChannelName);

		var toAdd = new List<UserNotificationPreference>();

		foreach (var patch in request.Preferences.Where(x => x.IsEnabled.IsSet))
		{
			UserNotificationPreference preference;
			if (preferences.TryGetValue(patch.ChannelName, out var value))
				preference = value;
			else
			{
				preference = UserNotificationPreference.Create(request.UserId, patch.ChannelName);
				toAdd.Add(preference);
			}

			patch.IsEnabled.Apply(preference.SetEnabled);

			if (preference.ChannelName == InAppRecipient.ChannelName)
				preference.Enable();
		}

		await unitOfWork.AddRangeAsync(toAdd, cancellationToken);
		return Unit.Value;
	}
}
