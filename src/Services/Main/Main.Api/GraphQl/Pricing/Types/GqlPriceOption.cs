using HotChocolate;
using Main.Api.GraphQl.Types;
using Pricing.Application.Dtos.Price;

namespace Main.Api.GraphQl.Pricing.Types;

[GraphQLName("PriceOption")]
public sealed record GqlPriceOption([property: GraphQLIgnore]
	PriceOptionDto Dto)
{
	[GraphQLName("score")]
	public decimal Score => Dto.Score;

	[GraphQLIgnore]
	public int CurrencyId => Dto.CurrencyId;

	[GraphQLName("sellPrice")]
	public decimal SellPrice => Dto.Price;

	[GraphQLName("markup")]
	public decimal Markup => Dto.Markup;

	[GraphQLName("forStorage")]
	public GqlStorage ForStorage => new(Dto.ForStorageCode);

	[GraphQLName("deliveryDays")]
	public int DeliveryDays => Dto.DeliveryDays;

	[GraphQLName("guaranteedDeliveryDays")]
	public int GuaranteedDeliveryDays => Dto.GuaranteedDeliveryDays;

	[GraphQLName("deliveryProbability")]
	public int DeliveryProbability => Dto.DeliveryProbability;

	[GraphQLName("offer")]
	public GqlPriceOffer Offer => new(Dto.PriceOffer);
}
