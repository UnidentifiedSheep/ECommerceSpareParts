using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Application.Dtos.Auth;
using Main.Entities.Auth;
using Main.Entities.Exceptions;

namespace Main.Api.GraphQl.Types.Auth;

[GraphQLName("Role")]
public record GqlRole
{
	private readonly RoleDto? _dto;

	public GqlRole(string systemName)
	{
		SystemName = RoleNames.Normalize(systemName);
	}

	public GqlRole(RoleDto role) : this(role.SystemName)
	{
		_dto = role;
	}

	[GraphQLName("systemName")]
	[Shareable]
	public string SystemName { get; }

	[GraphQLName("localizedName")]
	public async Task<string?> GetLocalizedNameAsync(
		IRoleByNameDataLoader loader, CancellationToken cancellationToken) =>
		(await GetRoleAsync(loader, cancellationToken)).LocalizedName;

	[GraphQLName("description")]
	public async Task<string?> GetDescriptionAsync(
		IRoleByNameDataLoader loader, CancellationToken cancellationToken) =>
		(await GetRoleAsync(loader, cancellationToken)).Description;

	[GraphQLName("createdAt")]
	public async Task<DateTime> GetCreatedAtAsync(
		IRoleByNameDataLoader loader, CancellationToken cancellationToken) =>
		(await GetRoleAsync(loader, cancellationToken)).CreatedAt;

	[GraphQLName("updatedAt")]
	public async Task<DateTime> GetUpdatedAtAsync(
		IRoleByNameDataLoader loader, CancellationToken cancellationToken) =>
		(await GetRoleAsync(loader, cancellationToken)).UpdatedAt;

	[GraphQLName("whoCreated")]
	public async Task<Guid?> GetWhoCreatedAsync(
		IRoleByNameDataLoader loader, CancellationToken cancellationToken) =>
		(await GetRoleAsync(loader, cancellationToken)).WhoCreated;

	[GraphQLName("whoUpdated")]
	public async Task<Guid?> GetWhoUpdatedAsync(
		IRoleByNameDataLoader loader, CancellationToken cancellationToken) =>
		(await GetRoleAsync(loader, cancellationToken)).WhoUpdated;

	[GraphQLName("permissions")]
	public async Task<IReadOnlyList<GqlPermission>> GetPermissionsAsync(
		IRolePermissionsByNameDataLoader loader, CancellationToken cancellationToken)
	{
		var permissions = await loader.LoadAsync(SystemName, cancellationToken) ??
			throw new RoleNotFoundException(SystemName);
		return permissions.Select(permission => new GqlPermission(permission)).ToArray();
	}

	private async Task<RoleDto> GetRoleAsync(IRoleByNameDataLoader loader, CancellationToken cancellationToken) =>
		_dto ?? await loader.LoadAsync(SystemName, cancellationToken)
		?? throw new RoleNotFoundException(SystemName);
}
