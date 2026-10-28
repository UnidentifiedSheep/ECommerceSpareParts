using HotChocolate;

namespace Pricing.Api.GraphQl.Mutations;

[GraphQLName("Mutation")]
public sealed class RootMutation
{
	[GraphQLName("markupGroups")]
	public MarkupGroupMutations MarkupGroups => new();

	[GraphQLName("priceAppliers")]
	public PriceApplierMutations PriceAppliers => new();
}
