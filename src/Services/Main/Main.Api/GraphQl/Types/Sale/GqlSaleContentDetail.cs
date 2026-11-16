using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Entities.Currency;
using Main.Entities.Exceptions;
using Main.Modules.Sale.Application.Dtos.Sale;

namespace Main.Api.GraphQl.Types.Sale;

[GraphQLName("SaleContentDetail")]
public sealed record GqlSaleContentDetail([property: GraphQLIgnore]
	SaleContentDetailDto Detail)
{
	[GraphQLName("id")]
	public int Id => Detail.Id;

	[GraphQLName("saleContentId")]
	public int SaleContentId => Detail.SaleContentId;

	[GraphQLName("storageContentId")]
	public int StorageContentId => Detail.StorageContentId;

	[GraphQLName("currency")]
	public async Task<GqlCurrency> GetCurrencyAsync(
		ICurrencyByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> new(await loader.LoadAsync(Detail.CurrencyId, cancellationToken)
			?? throw new CurrencyNotFoundException(Detail.CurrencyId));

	[GraphQLName("buyPrice")]
	public decimal BuyPrice => Detail.BuyPrice;

	[GraphQLName("count")]
	public int Count => Detail.Count;

	[GraphQLName("purchaseDatetime")]
	public DateTime PurchaseDatetime => Detail.PurchaseDatetime;
}
