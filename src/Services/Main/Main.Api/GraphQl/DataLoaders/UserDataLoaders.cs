using GreenDonut;
using Main.Modules.Identity.Application.Dtos.Users;
using Main.Modules.Identity.Application.Handlers.Users;
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
	public static async Task<IReadOnlyDictionary<Guid, UserRolesAndPermissions>>
		GetUserRolesAndPermissionsByIdAsync(
			IReadOnlyList<Guid> keys,
			ISender sender,
			CancellationToken cancellationToken)
		=> (await sender.Send(new GetUsersRolesAndPermissionsQuery(keys), cancellationToken)).Users;

	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<UserEmailDto>>> GetUserEmailsByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetUsersEmailsByIdsQuery(keys), cancellationToken)).Emails;


	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<UserPhoneDto>>> GetUserPhonesByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetUsersPhonesByIdsQuery(keys), cancellationToken)).Phones;
}
