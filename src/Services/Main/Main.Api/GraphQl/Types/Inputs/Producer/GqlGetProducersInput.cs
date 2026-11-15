using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Producer;

[GraphQLName("GetProducersInput")]
public sealed record GqlGetProducersInput
{
	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("searchTerm")]
	public string? SearchTerm { get; init; }

	[GraphQLName("ids")]
	public IReadOnlyCollection<int>? Ids { get; init; }
}
