using HotChocolate;
using Main.Application.Dtos.Purchase;

namespace Main.Api.GraphQl.Types.Inputs.Purchase;

[GraphQLName("EditPurchaseContentInput")]
public sealed record GqlEditPurchaseContentInput
{
	[GraphQLName("id")]
	public int? Id { get; init; }

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
	public EditPurchaseDto ToDto() => new()
	{
		Id = Id,
		ProductId = ProductId,
		Count = Count,
		Price = Price,
		CalculateLogistics = CalculateLogistics,
		Comment = Comment
	};
}
