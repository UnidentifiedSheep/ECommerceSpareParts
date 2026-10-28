using HotChocolate;
using Pricing.Application.Dtos.PriceApplier;
using Pricing.Enums;

namespace Pricing.Api.GraphQl.Types;

[GraphQLName("PriceApplierState")]
public sealed record GqlPriceApplierState([property: GraphQLIgnore] PriceApplierStateDto Dto)
{
	[GraphQLName("priceApplierSystemName")]
	public string PriceApplierSystemName => Dto.PriceApplierSystemName;

	[GraphQLName("usage")]
	public PriceOfferSourceType Usage => Dto.Usage;

	[GraphQLName("order")]
	public int Order => Dto.Order;

	[GraphQLName("enabled")]
	public bool Enabled => Dto.Enabled;
}
