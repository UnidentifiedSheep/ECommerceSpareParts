using HotChocolate;

namespace Main.Api.GraphQl.Pricing.Types.Inputs;

[GraphQLName("UpsertMarkupRangeInput")]
public sealed record GqlUpsertMarkupRangeInput
{
	[GraphQLName("rangeStart")]
	public required decimal RangeStart { get; init; }

	[GraphQLName("rangeEnd")]
	public required decimal RangeEnd { get; init; }

	[GraphQLName("markup")]
	public required decimal Markup { get; init; }
}
