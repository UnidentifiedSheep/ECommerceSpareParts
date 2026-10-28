using HotChocolate;

namespace Pricing.Api.GraphQl.Queries.Root;

[GraphQLName("Query")]
public sealed class Query
{
	[GraphQLName("markupGroups")]
	public MarkupGroupQueries MarkupGroups => new();
}
