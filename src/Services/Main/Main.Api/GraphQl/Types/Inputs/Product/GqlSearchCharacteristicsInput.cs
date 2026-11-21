using GraphQL.Common.Attributes;
using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("SearchCharacteristicsInput")]
public sealed record GqlSearchCharacteristicsInput
{
	[GraphQLName("searchTerm")]
	public string? SearchTerm { get; init; }

	[GraphQLName("sortBy")]
	[SortableBy<Main.Entities.Product.Product.Characteristic>]
	public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }

	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }
}
