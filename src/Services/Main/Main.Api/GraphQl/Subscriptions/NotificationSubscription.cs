using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.Types;

namespace Main.Api.GraphQl.Subscriptions;

[SubscriptionType]
public static partial class NotificationSubscription
{
	[EventStream("{ id }")]
	[GraphQLName("onNotificationCreated")]
	[RequireAllPermissions(PermissionCodes.NOTIFICATIONS_ME)]
	public static GqlNotification OnNotificationCreated()
		=> EventStream.Create<GqlNotification>();
}
