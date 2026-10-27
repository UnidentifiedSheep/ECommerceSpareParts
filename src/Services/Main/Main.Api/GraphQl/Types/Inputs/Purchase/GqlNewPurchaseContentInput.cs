using HotChocolate;
using Main.Application.Dtos.Purchase;

namespace Main.Api.GraphQl.Types.Inputs.Purchase;

[GraphQLName("NewPurchaseContentInput")]
public sealed record GqlNewPurchaseContentInput
{
	[GraphQLName("productId")]
	public required int ProductId { get; init; }

	[GraphQLName("count")]
	public required int Count { get; init; }

	[GraphQLName("price")]
	public required decimal Price { get; init; }

	[GraphQLName("calculateLogistics")]
	public required bool CalculateLogistics { get; init; }

	[GraphQLName("comment")]
	public string? Comment { get; init; }

	[GraphQLIgnore]
	public NewPurchaseContentDto ToDto() => new()
	{
		ProductId = ProductId,
		Count = Count,
		Price = Price,
		CalculateLogistics = CalculateLogistics,
		Comment = Comment
	};
}
