using HotChocolate;
using HotChocolate.Types.Composite;

namespace Pricing.Api.GraphQl.Types;

[GraphQLName("Currency")]
public class GqlCurrency(int id)
{
	[GraphQLName("id")]
	[Shareable]
	public int Id { get; } = id;
}
