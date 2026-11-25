namespace Pricing.Abstractions.Models;

public sealed record SupplierOfferSnapshot(
	int ProductId,
	int CurrencyId,
	decimal PurchasePrice,
	int DeliveryDays,
	int? GuaranteedDeliveryDays,
	int DeliveryProbability);
