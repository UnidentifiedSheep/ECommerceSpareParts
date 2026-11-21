using HotChocolate;
using Pricing.Application.Dtos.Price;

namespace Main.Api.GraphQl.Pricing.Types;

[GraphQLName("PriceOption")]
public sealed record GqlPriceOption([property: GraphQLIgnore]
	PriceOptionDto Dto)
{
	[GraphQLName("priceOfferId")]
	public Guid PriceOfferId => Dto.PriceOfferId;

	[GraphQLName("score")]
	public decimal Score => Dto.Score;

	[GraphQLIgnore]
	public int CurrencyId => Dto.CurrencyId;

	[GraphQLName("sellPrice")]
	public decimal SellPrice => Dto.Price;

	[GraphQLName("markup")]
	public decimal Markup => Dto.Markup;

	[GraphQLName("forStorage")]
	public string ForStorage => Dto.ForStorageCode;

	[GraphQLName("deliveryTime")]
	public TimeSpan DeliveryTime => Dto.DeliveryTime;

	[GraphQLName("guaranteedDeliveryTime")]
	public TimeSpan GuaranteedDeliveryTime => Dto.GuaranteedDeliveryTime;

	[GraphQLName("deliveryProbability")]
	public int DeliveryProbability => Dto.DeliveryProbability;

	[GraphQLName("offer")]
	public GqlPriceOffer Offer => new(Dto.PriceOffer);
}
