using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("ProductStorageContentsInput")]
public sealed record GqlProductStorageContentsInput
{
	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("storageCode")]
	public string? StorageCode { get; init; }

	[GraphQLName("showZeroCount")]
	public required bool ShowZeroCount { get; init; }
}
