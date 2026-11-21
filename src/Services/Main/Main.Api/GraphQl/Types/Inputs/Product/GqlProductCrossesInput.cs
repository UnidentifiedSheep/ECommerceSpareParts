using GraphQL.Common.Attributes;
using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("ProductCrossesInput")]
public sealed record GqlProductCrossesInput
{
	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("searchTerm")]
	public string? SearchTerm { get; init; }

	[GraphQLName("sortBy")]
	[SortableBy<Main.Entities.Product.Product.Product>]
	public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }
}
