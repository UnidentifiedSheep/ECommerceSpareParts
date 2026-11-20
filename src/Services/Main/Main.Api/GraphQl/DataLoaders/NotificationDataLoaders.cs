using GreenDonut;
using Main.Modules.Notifications.Application.Dtos;
using Main.Modules.Notifications.Application.Queries.Notifications;
using MediatR;
using Security.Core.Interfaces;

namespace Main.Api.GraphQl.DataLoaders;

public static class NotificationDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<int, InAppNotificationDto>> GetNotificationByIdAsync(
		IReadOnlyList<int> keys,
		IUserContext userContext,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(
				new GetNotificationsByIdsQuery(keys, userContext.UserId),
				cancellationToken))
			.Notifications;
}
