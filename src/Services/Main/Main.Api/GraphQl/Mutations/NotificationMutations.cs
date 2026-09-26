using Abstractions.Interfaces;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.Notification;
using Main.Application.Dtos.NotificationPreference;
using Main.Application.Handlers.NotificationPreferences.UpdateNotificationPreferences;
using Main.Application.Handlers.Notifications;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class NotificationMutations
{
	[GraphQLName("send")]
	[RequireAllPermissions(PermissionCodes.NOTIFICATIONS_ALL)]
	public async Task<bool> SendAsync(
		ISender sender,
		GqlSendNotificationInput input,
		CancellationToken cancellationToken)
		=> (await sender.Send(
			new SendNotificationCommand(input.UserId, input.Message),
			cancellationToken))
			.Succeeded;

	[GraphQLName("updatePreferences")]
	[RequireAllPermissions(PermissionCodes.NOTIFICATIONS_ME)]
	public async Task<bool> UpdatePreferencesAsync(
		IUserContext userContext,
		ISender sender,
		GqlUpdateNotificationPreferencesInput input,
		CancellationToken cancellationToken)
	{
		var preferences = input.Preferences
			.Select(x => new UserNotificationPreferencePatchDto
			{
				ChannelName = x.ChannelName,
				IsEnabled = x.IsEnabled
			})
			.ToArray();

		await sender.Send(
			new UpdateNotificationPreferencesCommand(userContext.UserId, preferences),
			cancellationToken);
		return true;
	}
}
