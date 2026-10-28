using HotChocolate;
using HotChocolate.Types.Composite;

namespace Pricing.Api.GraphQl.Queries.Root;

[GraphQLName("Query")]
public sealed class Query
{
	[GraphQLName("markupGroups")]
	public MarkupGroupQueries MarkupGroups => new();

	[GraphQLName("products")]
	[Shareable]
	public ProductQueries Products => new();

	[GraphQLName("priceAppliers")]
	public PriceApplierQueries PriceAppliers => new();
}
