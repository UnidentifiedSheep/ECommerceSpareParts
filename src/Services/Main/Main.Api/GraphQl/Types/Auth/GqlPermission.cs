using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Application.Dtos.Auth;
using Main.Entities.Exceptions;
using Security.Authorization;

namespace Main.Api.GraphQl.Types.Auth;

[GraphQLName("Permission")]
public record GqlPermission
{
	private readonly PermissionDto? _dto;

	public GqlPermission(string systemName)
	{
		SystemName = AuthorizationValueNormalizer.NormalizePermission(systemName);
	}

	public GqlPermission(PermissionDto permission) : this(permission.SystemName)
	{
		_dto = permission;
	}

	[GraphQLName("systemName")]
	[Shareable]
	public string SystemName { get; }

	[GraphQLName("name")]
	public async Task<string> GetNameAsync(
		IPermissionByNameDataLoader loader, CancellationToken cancellationToken) =>
		(await GetPermissionAsync(loader, cancellationToken)).Name;

	[GraphQLName("description")]
	public async Task<string> GetDescriptionAsync(
		IPermissionByNameDataLoader loader, CancellationToken cancellationToken) =>
		(await GetPermissionAsync(loader, cancellationToken)).Description;

	private async Task<PermissionDto> GetPermissionAsync(
		IPermissionByNameDataLoader loader, CancellationToken cancellationToken) =>
		_dto ?? await loader.LoadAsync(SystemName, cancellationToken) ??
		throw new PermissionNotFoundException(SystemName);
}
