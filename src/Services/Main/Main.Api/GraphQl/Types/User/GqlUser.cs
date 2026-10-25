using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Application.Dtos.Users;
using Main.Entities.Exceptions;

namespace Main.Api.GraphQl.Types.User;

[GraphQLName("User")]
public record GqlUser
{
	private readonly UserDto? _dto;

	public GqlUser(Guid id)
	{
		Id = id;
	}

	public GqlUser(UserDto user) : this(user.Id)
	{
		_dto = user;
	}

	[GraphQLName("id")]
	[Shareable]
	public Guid Id { get; }

	[GraphQLName("userName")]
	public async Task<string> GetUserNameAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(loader, cancellationToken)).UserName;

	[GraphQLName("normalizedUserName")]
	public async Task<string> GetNormalizedUserNameAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(loader, cancellationToken)).NormalizedUserName;

	[GraphQLName("createdAt")]
	public async Task<DateTime> GetCreatedAtAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(loader, cancellationToken)).CreatedAt;

	[GraphQLName("updatedAt")]
	public async Task<DateTime> GetUpdatedAtAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(loader, cancellationToken)).UpdatedAt;

	[GraphQLName("twoFactorEnabled")]
	public async Task<bool> GetTwoFactorEnabledAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(loader, cancellationToken)).TwoFactorEnabled;

	[GraphQLName("lockoutEnd")]
	public async Task<DateTime?> GetLockoutEndAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(loader, cancellationToken)).LockoutEnd;

	[GraphQLName("accessFailedCount")]
	public async Task<int> GetAccessFailedCountAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(loader, cancellationToken)).AccessFailedCount;

	[GraphQLName("lastLoginAt")]
	public async Task<DateTime?> GetLastLoginAtAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(loader, cancellationToken)).LastLoginAt;

	[GraphQLName("info")]
	public async Task<GqlUserInfo?> GetInfoAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken)
	{
		var user = await GetUserAsync(loader, cancellationToken);
		return user.UserInfo is null ? null : new GqlUserInfo(user.UserInfo);
	}

	[GraphQLName("roles")]
	[RequireAllPermissions(PermissionCodes.USERS_INFO_GET)]
	public async Task<IReadOnlyList<string>> GetRolesAsync(
		IUserRolesAndPermissionsByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await loader.LoadAsync(Id, cancellationToken) ?? throw new UserNotFoundException(Id)).Roles;

	[GraphQLName("permissions")]
	[RequireAllPermissions(PermissionCodes.USERS_INFO_GET)]
	public async Task<IReadOnlyList<string>> GetPermissionsAsync(
		IUserRolesAndPermissionsByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await loader.LoadAsync(Id, cancellationToken) ?? throw new UserNotFoundException(Id)).Permissions;

	private async Task<UserDto> GetUserAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> _dto ?? await loader.LoadAsync(Id, cancellationToken)
			?? throw new UserNotFoundException(Id);
}
