using Enums.Units;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("SetProductWeightInput")]
public sealed record GqlSetProductWeightInput
{
	[GraphQLName("productId")]
	public required int ProductId { get; init; }

	[GraphQLName("weight")]
	public required decimal Weight { get; init; }

	[GraphQLName("unit")]
	public required WeightUnit Unit { get; init; }
}
