using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Auth;
using Main.Api.GraphQl.Types.Inputs.Role;
using Main.Application.Handlers.Auth;
using Main.Application.Handlers.Auth.UpsertRole;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class RoleMutations
{
	[GraphQLName("upsert")]
	[RequireAllPermissions(PermissionCodes.ROLES_CREATE)]
	public async Task<GqlRole> UpsertAsync(
		GqlUpsertRoleInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new UpsertRoleCommand(input.Name, input.Description), cancellationToken);
		return new GqlRole(input.Name);
	}

	[GraphQLName("addPermission")]
	[RequireAllPermissions(PermissionCodes.ROLES_PERMISSIONS_CREATE)]
	public async Task<GqlRole> AddPermissionAsync(
		GqlAddPermissionToRoleInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new AddPermissionToRoleCommand(input.RoleName, input.PermissionName),
			cancellationToken);
		return new GqlRole(input.RoleName);
	}
}
