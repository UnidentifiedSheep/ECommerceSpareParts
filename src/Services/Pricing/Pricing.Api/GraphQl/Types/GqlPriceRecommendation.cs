using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Pricing.Application.Dtos.Price;

namespace Pricing.Api.GraphQl.Types;

[GraphQLName("PriceRecommendation")]
public sealed record GqlPriceRecommendation([property: GraphQLIgnore] PriceRecommendationDto Dto)
{
	[GraphQLName("quantity")]
	public int Quantity => Dto.Quantity;

	[GraphQLName("missingQuantity")]
	public int MissingQuantity => Dto.MissingQuantity;

	[GraphQLName("currency")]
	public GqlCurrency Currency => new(Dto.CurrencyId);

	[GraphQLName("unitPrice")]
	public decimal? UnitPrice => Dto.UnitPrice;

	[GraphQLName("totalPrice")]
	public decimal? TotalPrice => Dto.TotalPrice;

	[GraphQLName("items")]
	public IReadOnlyList<GqlPriceRecommendationItem> Items =>
		Dto.Items.Select(item => new GqlPriceRecommendationItem(item)).ToArray();
}

[GraphQLName("PriceRecommendationItem")]
public sealed record GqlPriceRecommendationItem([property: GraphQLIgnore] PriceRecommendationItemDto Dto)
{
	[GraphQLName("quantity")]
	public int Quantity => Dto.Quantity;

	[GraphQLName("unitPrice")]
	public decimal UnitPrice => Dto.Option.Price;

	[GraphQLName("option")]
	[RequireAllPermissions(PermissionCodes.PRICES_GET_DETAILED)]
	public GqlPriceOption Option => new(Dto.Option);
}
