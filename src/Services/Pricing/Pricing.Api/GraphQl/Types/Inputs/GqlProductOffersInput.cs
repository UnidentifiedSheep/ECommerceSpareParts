using GraphQL.Common.Types;
using HotChocolate;
using Pricing.Enums;

namespace Pricing.Api.GraphQl.Types.Inputs;

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
	public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }
}
