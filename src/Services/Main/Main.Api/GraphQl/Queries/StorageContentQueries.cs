using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Storage;
using Main.Application.Handlers.StorageContents.GetStorageContents;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class StorageContentQueries
{
	[GraphQLName("search")]
	[RequireAnyPermission(PermissionCodes.STORAGES_CONTENT_GET_ALL)]
	public async Task<IReadOnlyList<GqlStorageContent>> SearchAsync(
		GqlStorageContentsSearchInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetStorageContentsQuery(
				input.ProductId,
				input.StorageCode,
				input.SortBy?.Select(x => x.ToSortExpression()).ToArray() ?? [],
				input.Pagination,
				input.ShowZeroCount),
			cancellationToken);

		return result.Content.Select(x => new GqlStorageContent(x)).ToArray();
	}
}
