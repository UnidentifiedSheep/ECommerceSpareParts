using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Users;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Extensions.QueryExtensions;

public static class UserAuthorizationQueryExtensions
{
	private static readonly Expression<Func<User, UserAccessData>> AccessProjection = user => new UserAccessData
	{
		UserId = user.Id,
		Roles = user.Roles.Select(role => new RoleAccessData
		{
			Name = role.RoleName,
			Permissions = role.Role.RolePermissions.Select(permission => permission.PermissionName).ToArray()
		}).ToArray(),
		DirectPermissions = user.Permissions.Select(permission => permission.Permission).ToArray()
	};

	public static async Task<UserAuthorizationDto?> GetUserAuthorizationAsync(
		this IReadRepository<User, Guid> repository,
		Guid userId,
		IProjectionProvider<User, UserDto> userProjection,
		CancellationToken cancellationToken = default)
	{
		var userToDto = userProjection.Projection;
		var result = await repository.Query
			.Where(user => user.Id == userId)
			.AsExpandable()
			.AsSingleQuery()
			.Select(user => new
			{
				User = userToDto.Invoke(user),
				Access = AccessProjection.Invoke(user)
			})
			.FirstOrDefaultAsync(cancellationToken);

		if (result is null)
			return null;

		var access = ExtractAccess(result.Access);
		return new UserAuthorizationDto(result.User, access.Roles, access.Permissions);
	}

	public static async Task<UserRolesAndPermissions?> GetUserRolesAndPermissionsAsync(
		this IReadRepository<User, Guid> repository,
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		var users = await repository.GetUsersRolesAndPermissionsAsync([userId], cancellationToken);
		return users.GetValueOrDefault(userId);
	}

	public static async Task<IReadOnlyDictionary<Guid, UserRolesAndPermissions>> GetUsersRolesAndPermissionsAsync(
		this IReadRepository<User, Guid> repository,
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		var ids = userIds.Distinct().ToArray();
		if (ids.Length == 0)
			return new Dictionary<Guid, UserRolesAndPermissions>();

		return await repository.Query
			.Where(user => ids.Contains(user.Id))
			.AsSingleQuery()
			.Select(AccessProjection)
			.ToDictionaryAsync(user => user.UserId, user => ExtractAccess(user), cancellationToken);
	}

	private static UserRolesAndPermissions ExtractAccess(UserAccessData user) => new()
	{
		Roles = user.Roles.Select(role => role.Name).ToArray(),
		Permissions = user.DirectPermissions
			.Concat(user.Roles.SelectMany(role => role.Permissions))
			.Distinct()
			.ToArray()
	};

	private sealed record UserAccessData
	{
		public required Guid UserId { get; init; }
		public required RoleAccessData[] Roles { get; init; }
		public required string[] DirectPermissions { get; init; }
	}

	private sealed record RoleAccessData
	{
		public required string Name { get; init; }
		public required string[] Permissions { get; init; }
	}
}
