using HotChocolate;
using Pricing.Entities.Enums;

namespace Main.Api.GraphQl.Pricing.Types.Inputs;

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
