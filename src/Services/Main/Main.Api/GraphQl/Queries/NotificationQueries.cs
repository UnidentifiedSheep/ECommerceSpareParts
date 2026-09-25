using Abstractions.Interfaces;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types;

namespace Main.Api.GraphQl.Queries;

public sealed class NotificationQueries
{
	[Lookup]
	[GraphQLName("byId")] //lookup for auth user.
	[RequireAllPermissions(PermissionCodes.NOTIFICATIONS_ME)]
	public async Task<GqlNotification?> GetByIdAsync(
		INotificationByIdDataLoader loader,
		int id,
		CancellationToken cancellationToken)
	{
		var res = await loader.LoadAsync(id, cancellationToken);
		return res == null ? null : new GqlNotification(res);
	}
}
