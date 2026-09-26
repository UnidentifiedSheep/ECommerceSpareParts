using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.Notification;
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
}
