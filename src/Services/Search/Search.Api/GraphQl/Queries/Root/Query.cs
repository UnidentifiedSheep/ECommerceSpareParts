using HotChocolate;
using HotChocolate.Types.Composite;

namespace Search.Api.GraphQl.Queries.Root;

public sealed class Query
{
	[GraphQLName("catalogue")]
	public CatalogueQueries Catalogue => new();

	[GraphQLName("producers")]
	[Shareable]
	public ProducerQueries Producer => new();
}
