using HotChocolate;
using Pricing.Application.Dtos.Price;
using Pricing.Enums;

namespace Pricing.Api.GraphQl.Types;

[GraphQLName("PriceOffer")]
public sealed record GqlPriceOffer([property: GraphQLIgnore] PriceOfferDto Dto)
{
	[GraphQLName("id")]
	public Guid Id => Dto.Id;

	[GraphQLName("product")]
	public GqlProduct Product => new(Dto.ProductId);

	[GraphQLName("currency")]
	public GqlCurrency Currency => new(Dto.CurrencyId);

	[GraphQLName("offerForStorage")]
	public string OfferForStorage => Dto.OfferForStorage;

	[GraphQLName("purchasePrice")]
	public decimal PurchasePrice => Dto.PurchasePrice;

	[GraphQLName("source")]
	public PriceOfferSource Source => Dto.Source;

	[GraphQLName("availableQuantity")]
	public int AvailableQuantity => Dto.AvailableQuantity;

	[GraphQLName("minimumPurchaseQuantity")]
	public int MinimumPurchaseQuantity => Dto.MinimumPurchaseQuantity;

	[GraphQLName("quantityCoefficient")]
	public int QuantityCoefficient => Dto.QuantityCoefficient;

	[GraphQLName("daysToRefund")]
	public int DaysToRefund => Dto.DaysToRefund;

	[GraphQLName("deliveryDate")]
	public DateTime? DeliveryDate => Dto.DeliveryDate;

	[GraphQLName("guaranteedDeliveryDate")]
	public DateTime? GuaranteedDeliveryDate => Dto.GuaranteedDeliveryDate;

	[GraphQLName("deliveryProbability")]
	public int DeliveryProbability => Dto.DeliveryProbability;

	[GraphQLName("orderTill")]
	public DateTime? OrderTill => Dto.OrderTill;

	[GraphQLName("expiresAt")]
	public DateTime ExpiresAt => Dto.ExpiresAt;
}
