using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Pricing.Types.Inputs;

[GraphQLName("SearchMarkupGroupsInput")]
public sealed record GqlSearchMarkupGroupsInput
{
	[GraphQLName("searchTerm")]
	public string? SearchTerm { get; init; }

	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }
}
