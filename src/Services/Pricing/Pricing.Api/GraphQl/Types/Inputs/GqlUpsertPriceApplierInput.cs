using HotChocolate;

namespace Pricing.Api.GraphQl.Types.Inputs;

[GraphQLName("UpsertPriceApplierInput")]
public sealed record GqlUpsertPriceApplierInput
{
	[GraphQLName("systemName")]
	public required string SystemName { get; init; }

	[GraphQLName("name")]
	public string? Name { get; init; }

	[GraphQLName("dslLogic")]
	public string? DslLogic { get; init; }

	[GraphQLName("states")]
	public required IReadOnlyList<GqlUpsertPriceApplierStateInput> States { get; init; }
}
