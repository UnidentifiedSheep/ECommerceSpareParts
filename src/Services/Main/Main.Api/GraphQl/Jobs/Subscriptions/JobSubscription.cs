using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.Jobs.Types;

namespace Main.Api.GraphQl.Jobs.Subscriptions;

[SubscriptionType]
public static partial class JobSubscription
{
	[EventStream("job { id }")]
	[GraphQLName("onJobUpdated")]
	[RequireAllPermissions(PermissionCodes.JOBS_GET)]
	public static GqlJobUpdated OnJobUpdated() => EventStream.Create<GqlJobUpdated>();
}

[GraphQLName("JobUpdated")]
public sealed record GqlJobUpdated(
	[property: GraphQLName("job")]
	GqlJob Job);
