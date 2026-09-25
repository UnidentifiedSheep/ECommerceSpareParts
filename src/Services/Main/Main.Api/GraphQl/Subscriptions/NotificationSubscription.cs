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
	public static GqlNotification OnNotificationCreated()
		=> EventStream.Create<GqlNotification>();
}
