using HotChocolate;
using Main.Application.Dtos.Sale;

namespace Main.Api.GraphQl.Types.Inputs.Sale;

[GraphQLName("NewSaleContentInput")]
public sealed record GqlNewSaleContentInput
{
	[GraphQLName("productId")]
	public required int ProductId { get; init; }

	[GraphQLName("count")]
	public required int Count { get; init; }

	[GraphQLName("price")]
	public required decimal Price { get; init; }

	[GraphQLName("priceWithDiscount")]
	public required decimal PriceWithDiscount { get; init; }

	[GraphQLName("comment")]
	public string? Comment { get; init; }

	[GraphQLIgnore]
	public NewSaleContentDto ToDto() => new()
	{
		ProductId = ProductId,
		Count = Count,
		Price = Price,
		PriceWithDiscount = PriceWithDiscount,
		Comment = Comment
	};
}
