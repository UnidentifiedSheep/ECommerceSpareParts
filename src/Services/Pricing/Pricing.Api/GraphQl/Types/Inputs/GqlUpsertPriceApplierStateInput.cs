using HotChocolate;
using Pricing.Enums;

namespace Pricing.Api.GraphQl.Types.Inputs;

[GraphQLName("UpsertPriceApplierStateInput")]
public sealed record GqlUpsertPriceApplierStateInput
{
	[GraphQLName("usage")]
	public required PriceOfferSourceType Usage { get; init; }

	[GraphQLName("order")]
	public int? Order { get; init; }

	[GraphQLName("enabled")]
	public required bool Enabled { get; init; }
}
