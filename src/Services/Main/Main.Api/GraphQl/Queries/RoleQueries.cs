using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Auth;
using Main.Api.GraphQl.Types.Inputs.Role;
using Main.Entities.Auth;
using Main.Modules.Identity.Application.Queries.Auth.GetRoles;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class RoleQueries
{
	[GraphQLName("byName")]
	[Lookup]
	[RequireAllPermissions(PermissionCodes.ROLES_GET)]
	public async Task<GqlRole?> GetByNameAsync(
		[Is("systemName")] string name,
		IRoleByNameDataLoader loader,
		CancellationToken cancellationToken)
	{
		var role = await loader.LoadAsync(RoleNames.Normalize(name), cancellationToken);
		return role is null ? null : new GqlRole(role);
	}

	[GraphQLName("list")]
	[RequireAllPermissions(PermissionCodes.ROLES_GET)]
	public async Task<IReadOnlyList<GqlRole>> ListAsync(
		GqlGetRolesInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetRolesQuery(input.SearchTerm, input.Pagination),
			cancellationToken);

		return result.Roles.Select(role => new GqlRole(role)).ToArray();
	}
}
