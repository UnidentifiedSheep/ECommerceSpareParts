using GreenDonut;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Users;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class UserDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, UserDto>> GetUserByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetUsersByIdsQuery(keys), cancellationToken)).Users;

	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, UserRolesAndPermissions>> GetUserRolesAndPermissionsByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetUsersRolesAndPermissionsQuery(keys), cancellationToken)).Users;
}
