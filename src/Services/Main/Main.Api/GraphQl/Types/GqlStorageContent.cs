using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Dtos.Storage;
using Main.Entities.Exceptions;

namespace Main.Api.GraphQl.Types;

[GraphQLName("StorageContent")]
public sealed record GqlStorageContent(
	[property: GraphQLIgnore]
	StorageContentDto Content)
{
	[GraphQLName("id")]
	public int Id => Content.Id;

	[GraphQLName("count")]
	public int Count => Content.Count;

	[GraphQLName("buyPrice")]
	public decimal BuyPrice => Content.BuyPrice;

	[GraphQLName("purchaseDatetime")]
	public DateTime PurchaseDatetime => Content.PurchaseDatetime;

	[GraphQLName("rowVersion")]
	public uint RowVersion => Content.RowVersion;

	[GraphQLName("currency")]
	public async Task<GqlCurrency> GetCurrencyAsync(
		ICurrencyByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> new(await loader.LoadAsync(Content.CurrencyId, cancellationToken)
			?? throw new CurrencyNotFoundException(Content.CurrencyId));

	[GraphQLName("storage")]
	public GqlStorage Storage => new(Content.StorageCode);

	[GraphQLName("product")]
	public GqlProduct Product => new(Content.ProductId);
}
