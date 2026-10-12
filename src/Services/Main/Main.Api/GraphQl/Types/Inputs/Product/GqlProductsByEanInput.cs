using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("ProductsByEanInput")]
public sealed record GqlProductsByEanInput
{
	[GraphQLName("ean")]
	public required string Ean { get; init; }

	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }
}
