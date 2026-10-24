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
		(await GetUserAsync(this, loader, cancellationToken)).UserName;

	[GraphQLName("normalizedUserName")]
	public async Task<string> GetNormalizedUserNameAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(this, loader, cancellationToken)).NormalizedUserName;

	[GraphQLName("createdAt")]
	public async Task<DateTime> GetCreatedAtAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(this, loader, cancellationToken)).CreatedAt;

	[GraphQLName("updatedAt")]
	public async Task<DateTime> GetUpdatedAtAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(this, loader, cancellationToken)).UpdatedAt;

	[GraphQLName("twoFactorEnabled")]
	public async Task<bool> GetTwoFactorEnabledAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(this, loader, cancellationToken)).TwoFactorEnabled;

	[GraphQLName("lockoutEnd")]
	public async Task<DateTime?> GetLockoutEndAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(this, loader, cancellationToken)).LockoutEnd;

	[GraphQLName("accessFailedCount")]
	public async Task<int> GetAccessFailedCountAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(this, loader, cancellationToken)).AccessFailedCount;

	[GraphQLName("lastLoginAt")]
	public async Task<DateTime?> GetLastLoginAtAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetUserAsync(this, loader, cancellationToken)).LastLoginAt;

	[GraphQLName("info")]
	public async Task<GqlUserInfo?> GetInfoAsync(
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken)
	{
		var user = await GetUserAsync(this, loader, cancellationToken);
		return user.UserInfo is null ? null : new GqlUserInfo(user.UserInfo);
	}

	private static async Task<UserDto> GetUserAsync(
		GqlUser user,
		IUserByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> user._dto ?? await loader.LoadAsync(user.Id, cancellationToken)
			?? throw new UserNotFoundException(user.Id);
}
