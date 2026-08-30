using HotChocolate;
using HotChocolate.Types.Composite;

namespace Search.Api.GraphQl.Types;

[GraphQLName("Producer")]
public record GqlProducer(
	[property: GraphQLName("id")]
	[property: Shareable]
	int Id)
{
}
