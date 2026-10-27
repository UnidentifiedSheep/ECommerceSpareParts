using HotChocolate;
using HotChocolate.Types.Composite;

namespace Pricing.Api.GraphQl.Types;


[GraphQLName("Product")]
public record GqlProduct
{
	[GraphQLName("id")]
	[Shareable]
	public int Id { get; }

	public GqlProduct(int id)
	{
		Id = id;
	}


}
