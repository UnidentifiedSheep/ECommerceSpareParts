using HotChocolate;
using HotChocolate.Types.Composite;
using Pricing.Api.GraphQl.Types;

namespace Pricing.Api.GraphQl.Queries;

public sealed class ProductQueries
{
	[GraphQLName("byId")]
	[Lookup]
	[Internal]
	public GqlProduct? GetProductById(int id) => new(id);
}
