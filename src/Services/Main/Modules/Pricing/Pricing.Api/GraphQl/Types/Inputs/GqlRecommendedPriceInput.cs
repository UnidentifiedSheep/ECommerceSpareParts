using HotChocolate;
using Pricing.Enums;

namespace Pricing.Api.GraphQl.Types.Inputs;

[GraphQLName("RecommendedPriceInput")]
public sealed record GqlRecommendedPriceInput
{
	[GraphQLName("currencyId")]
	public required int CurrencyId { get; init; }

	[GraphQLName("storageCode")]
	public required string StorageCode { get; init; }

	[GraphQLName("quantity")]
	public required int Quantity { get; init; }

	[GraphQLName("sources")]
	public IReadOnlyCollection<PriceOfferSource> Sources { get; init; } = [];
}
