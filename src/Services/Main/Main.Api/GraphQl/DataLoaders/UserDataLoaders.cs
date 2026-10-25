using GreenDonut;
using Main.Application.Dtos.Users;
using Main.Application.Dtos.Storage;
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

	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<StorageDto>>> GetUserStoragesByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetUsersStoragesByIdsQuery(keys), cancellationToken)).Storages;

}
