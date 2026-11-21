using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Storage;
using Main.Modules.Storage.Application.Queries.Storages.GetStorage;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class StorageQueries
{
	[GraphQLName("byCode")]
	[Lookup]
	[RequireAllPermissions(PermissionCodes.STORAGES_GET)]
	public async Task<GqlStorage?> GetByCodeAsync(
		string code,
		IStorageByCodeDataLoader loader,
		CancellationToken cancellationToken)
	{
		var storage = await loader.LoadAsync(code, cancellationToken);
		return storage is null ? null : new GqlStorage(storage);
	}

	[GraphQLName("list")]
	[RequireAllPermissions(PermissionCodes.STORAGES_GET)]
	public async Task<IReadOnlyList<GqlStorage>> ListAsync(
		GqlGetStoragesInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetStoragesQuery(
				input.Pagination,
				input.SearchTerm,
				input.Type),
			cancellationToken);

		return result.Storages.Select(storage => new GqlStorage(storage)).ToArray();
	}
}
