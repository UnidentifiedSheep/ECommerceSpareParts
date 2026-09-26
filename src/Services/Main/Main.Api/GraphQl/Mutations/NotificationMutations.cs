using Abstractions.Interfaces;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.Notification;
using Main.Application.Dtos.NotificationPreference;
using Main.Application.Handlers.NotificationPreferences.UpsertNotificationPreferences;
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

	[GraphQLName("upsertPreferences")]
	[RequireAllPermissions(PermissionCodes.NOTIFICATIONS_ME)]
	public async Task<bool> UpsertPreferencesAsync(
		IUserContext userContext,
		ISender sender,
		GqlUpsertNotificationPreferencesInput input,
		CancellationToken cancellationToken)
	{
		var preferences = input.Preferences
			.Select(x => new UpsertUserNotificationPreferenceDto
			{
				ChannelName = x.ChannelName,
				IsEnabled = x.IsEnabled
			})
			.ToArray();

		await sender.Send(
			new UpsertNotificationPreferencesCommand(userContext.UserId, preferences),
			cancellationToken);
		return true;
	}
}
