using GraphQL.Common.Attributes;
using GraphQL.Common.Types;
using HotChocolate;
using Pricing.Entities.Enums;

namespace Main.Api.GraphQl.Pricing.Types.Inputs;

[GraphQLName("ProductOffersInput")]
public sealed record GqlProductOffersInput
{
	[GraphQLName("currencyId")]
	public required int CurrencyId { get; init; }

	[GraphQLName("storageCode")]
	public required string StorageCode { get; init; }

	[GraphQLName("sources")]
	public IReadOnlyCollection<PriceOfferSource> Sources { get; init; } = [];

	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("sortBy")]
	[SortableBy<global::Pricing.Entities.Offers.ProductPriceOption>]
	public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }
}
