using Abstractions.Interfaces;
using GreenDonut;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Notifications;
using MediatR;

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
