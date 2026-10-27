using HotChocolate;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Dtos.Purchase;

namespace Main.Api.GraphQl.Types;

[GraphQLName("PurchaseContent")]
public sealed record GqlPurchaseContent([property: GraphQLIgnore] PurchaseContentDto Content)
{
	[GraphQLName("id")]
	public int Id => Content.Id;

	[GraphQLName("count")]
	public int Count => Content.Count;

	[GraphQLName("price")]
	public decimal Price => Content.Price;

	[GraphQLName("totalSum")]
	public decimal TotalSum => Content.TotalSum;

	[GraphQLName("comment")]
	public string? Comment => Content.Comment;

	[GraphQLName("product")]
	public GqlProduct Product => new(Content.Product);

	[GraphQLName("logistics")]
	public GqlPurchaseContentLogistic? Logistics =>
		Content.ContentLogistics is { } logistics ? new(logistics) : null;
}
