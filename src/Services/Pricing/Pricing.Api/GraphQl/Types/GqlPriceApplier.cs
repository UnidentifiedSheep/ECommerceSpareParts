using HotChocolate;
using Pricing.Application.Dtos.PriceApplier;

namespace Pricing.Api.GraphQl.Types;

[GraphQLName("PriceApplier")]
public sealed record GqlPriceApplier([property: GraphQLIgnore] PriceApplierDto Dto)
{
	[GraphQLName("systemName")]
	public string SystemName => Dto.SystemName;

	[GraphQLName("name")]
	public string Name => Dto.Name;

	[GraphQLName("isDynamic")]
	public bool IsDynamic => Dto.IsDynamic;

	[GraphQLName("dslLogic")]
	public string? DslLogic => Dto.DslLogic;

	[GraphQLName("states")]
	public IReadOnlyList<GqlPriceApplierState> States => Dto.States.Select(state => new GqlPriceApplierState(state)).ToArray();
}
