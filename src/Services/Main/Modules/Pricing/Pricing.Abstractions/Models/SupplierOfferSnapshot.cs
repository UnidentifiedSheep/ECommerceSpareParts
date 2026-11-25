namespace Pricing.Abstractions.Models;

public sealed record SupplierOfferSnapshot(
	int ProductId,
	int CurrencyId,
	decimal PurchasePrice,
	DateTime? DeliveryDate,
	DateTime? GuaranteedDeliveryDate,
	int DeliveryProbability);
