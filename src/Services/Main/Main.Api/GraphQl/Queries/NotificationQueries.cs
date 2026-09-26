using Abstractions.Interfaces;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Notification;
using Main.Api.GraphQl.Types.Notification;
using Main.Application.Handlers.NotificationPreferences;
using Main.Application.Handlers.Notifications.GetNotifications;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class NotificationQueries
{
	[Lookup]
	[GraphQLName("byId")]
	[RequireAllPermissions(PermissionCodes.NOTIFICATIONS_ME)]
	public async Task<GqlNotification?> GetByIdAsync(
		INotificationByIdDataLoader loader,
		int id,
		CancellationToken cancellationToken)
	{
		var res = await loader.LoadAsync(id, cancellationToken);
		return res == null ? null : new GqlNotification(res);
	}

	[GraphQLName("myPreferences")]
	[RequireAllPermissions(PermissionCodes.NOTIFICATIONS_ME)]
	public async Task<IReadOnlyList<GqlNotificationPreference>> GetMyPreferencesAsync(
		IUserContext context,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetNotificationPreferencesQuery(context.UserId), cancellationToken))
			.Preferences
			.Select(x => new GqlNotificationPreference(x))
			.ToList();

	[GraphQLName("list")]
	[RequireAllPermissions(PermissionCodes.NOTIFICATIONS_ME)]
	public async Task<IReadOnlyList<GqlNotification>> GetNotificationsAsync(
		IUserContext context,
		GqlListNotificationsInput input,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetNotificationsQuery(context.UserId, input.Cursor), cancellationToken))
			.Notifications
			.Select(x => new GqlNotification(x))
			.ToList();
}
