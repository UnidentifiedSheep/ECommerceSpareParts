using HotChocolate;
using Main.Application.Dtos.Sale;

namespace Main.Api.GraphQl.Types;

[GraphQLName("SaleContentDetail")]
public sealed record GqlSaleContentDetail([property: GraphQLIgnore] SaleContentDetailDto Detail)
{
	[GraphQLName("id")]
	public int Id => Detail.Id;

	[GraphQLName("saleContentId")]
	public int SaleContentId => Detail.SaleContentId;

	[GraphQLName("storageContentId")]
	public int StorageContentId => Detail.StorageContentId;

	[GraphQLName("currency")]
	public GqlCurrency Currency => new(Detail.Currency);

	[GraphQLName("buyPrice")]
	public decimal BuyPrice => Detail.BuyPrice;

	[GraphQLName("count")]
	public int Count => Detail.Count;

	[GraphQLName("purchaseDatetime")]
	public DateTime PurchaseDatetime => Detail.PurchaseDatetime;
}
