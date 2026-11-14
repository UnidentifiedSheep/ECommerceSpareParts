using GraphQL.Common.Types;
using HotChocolate;

namespace Pricing.Api.GraphQl.Types.Inputs;

[GraphQLName("SearchMarkupGroupsInput")]
public sealed record GqlSearchMarkupGroupsInput
{
	[GraphQLName("searchTerm")]
	public string? SearchTerm { get; init; }

	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }
}
