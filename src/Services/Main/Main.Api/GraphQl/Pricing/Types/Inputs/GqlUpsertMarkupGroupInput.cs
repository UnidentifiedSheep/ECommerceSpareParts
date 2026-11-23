using HotChocolate;

namespace Main.Api.GraphQl.Pricing.Types.Inputs;

[GraphQLName("UpsertMarkupGroupInput")]
public sealed record GqlUpsertMarkupGroupInput
{
	[GraphQLName("id")]
	public int? Id { get; init; }

	[GraphQLName("name")]
	public string? Name { get; init; }

	[GraphQLName("currencyId")]
	public required int CurrencyId { get; init; }

	[GraphQLName("ranges")]
	public required IReadOnlyList<GqlUpsertMarkupRangeInput> Ranges { get; init; }
}
