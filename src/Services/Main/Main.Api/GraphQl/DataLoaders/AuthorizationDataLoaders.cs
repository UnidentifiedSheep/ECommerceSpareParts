using GreenDonut;
using Main.Application.Dtos.Auth;
using Main.Application.Handlers.Auth;
using Main.Application.Handlers.Auth.GetPermissions;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class AuthorizationDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<string, RoleDto>> GetRoleByNameAsync(
		IReadOnlyList<string> keys, ISender sender, CancellationToken cancellationToken) =>
		(await sender.Send(new GetRolesByNamesQuery(keys), cancellationToken)).Roles;

	[DataLoader]
	public static async Task<IReadOnlyDictionary<string, PermissionDto>> GetPermissionByNameAsync(
		IReadOnlyList<string> keys, ISender sender, CancellationToken cancellationToken) =>
		(await sender.Send(new GetPermissionsQuery(), cancellationToken)).Permissions
			.ToDictionary(permission => permission.SystemName);

	[DataLoader]
	public static async Task<IReadOnlyDictionary<string, IReadOnlyList<PermissionDto>>> GetRolePermissionsByNameAsync(
		IReadOnlyList<string> keys, ISender sender, CancellationToken cancellationToken) =>
		(await sender.Send(new GetRolesPermissionsByNamesQuery(keys), cancellationToken)).Permissions;
}
