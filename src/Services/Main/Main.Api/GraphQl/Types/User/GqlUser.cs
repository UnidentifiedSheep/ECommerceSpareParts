using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Auth;
using Main.Api.GraphQl.Types.Inputs.User;
using Main.Application.Handlers.Organizations.GetOrganizations;
using MediatR;
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

	[GraphQLName("emails")]
	[RequireAllPermissions(PermissionCodes.USERS_INFO_GET)]
	public async Task<IReadOnlyList<GqlUserEmail>> GetEmailsAsync(
		IUserEmailsByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await loader.LoadAsync(Id, cancellationToken) ?? [])
			.Select(email => new GqlUserEmail(email))
			.ToArray();

	[GraphQLName("phones")]
	[RequireAllPermissions(PermissionCodes.USERS_INFO_GET)]
	public async Task<IReadOnlyList<GqlUserPhone>> GetPhonesAsync(
		IUserPhonesByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await loader.LoadAsync(Id, cancellationToken) ?? [])
			.Select(phone => new GqlUserPhone(phone))
			.ToArray();


	[GraphQLName("storages")]
	[RequireAllPermissions(PermissionCodes.USERS_STORAGES_GET)]
	public async Task<IReadOnlyList<GqlStorage>> GetStoragesAsync(
		IUserStoragesByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await loader.LoadAsync(Id, cancellationToken) ?? [])
			.Select(storage => new GqlStorage(storage))
			.ToArray();

	[GraphQLName("organizations")]
	[RequireAllPermissions(PermissionCodes.ORGANIZATIONS_GET)]
	public async Task<IReadOnlyList<GqlOrganization>> GetOrganizationsAsync(
		GqlUserOrganizationsInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetOrganizationsQuery(
				input.Pagination,
				input.SortBy?.Select(sort => sort.ToSortExpression()).ToArray() ?? [],
				input.SearchTerm,
				Id,
				input.Ids ?? [],
				input.Types ?? [],
				input.ShowHidden ?? false),
			cancellationToken);

		return result.Organizations.Select(organization => new GqlOrganization(organization)).ToArray();
	}

	[GraphQLName("roles")]
	[RequireAllPermissions(PermissionCodes.USERS_INFO_GET)]
	public async Task<IReadOnlyList<GqlRole>> GetRolesAsync(
		IUserRolesAndPermissionsByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await loader.LoadAsync(Id, cancellationToken) ?? throw new UserNotFoundException(Id)).Roles
			.Select(role => new GqlRole(role)).ToArray();

	[GraphQLName("permissions")]
	[RequireAllPermissions(PermissionCodes.USERS_INFO_GET)]
	public async Task<IReadOnlyList<GqlPermission>> GetPermissionsAsync(
		IUserRolesAndPermissionsByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await loader.LoadAsync(Id, cancellationToken) ?? throw new UserNotFoundException(Id)).Permissions
			.Select(permission => new GqlPermission(permission)).ToArray();

	private async Task<UserDto> GetUserAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> _dto ?? await loader.LoadAsync(Id, cancellationToken)
			?? throw new UserNotFoundException(Id);
}
