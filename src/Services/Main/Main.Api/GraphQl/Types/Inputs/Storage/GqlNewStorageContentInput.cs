using HotChocolate;
using Main.Modules.Storage.Application.Dtos.Storage;

namespace Main.Api.GraphQl.Types.Inputs.Storage;

[GraphQLName("NewStorageContentInput")]
public sealed record GqlNewStorageContentInput
{
	[GraphQLName("productId")]
	public required int ProductId { get; init; }

	[GraphQLName("currencyId")]
	public required int CurrencyId { get; init; }

	[GraphQLName("buyPrice")]
	public required decimal BuyPrice { get; init; }

	[GraphQLName("count")]
	public required int Count { get; init; }

	[GraphQLName("purchaseDate")]
	public required DateTime PurchaseDate { get; init; }

	public NewStorageContentDto ToDto()
		=> new()
		{
			ProductId = ProductId,
			CurrencyId = CurrencyId,
			BuyPrice = BuyPrice,
			Count = Count,
			PurchaseDate = PurchaseDate
		};
}
