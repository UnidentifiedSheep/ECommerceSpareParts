using Application.Common.Interfaces.Settings;
using Enums;
using Integrations.Supplier.Models;
using Main.Modules.Currency.Abstractions.Interfaces;
using Main.Modules.Logistics.Abstractions.Interfaces;
using Pricing.Application.Extensions;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models;
using Pricing.Entities.Offers;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services.Pricing;

public class SupplierOfferConverterService(
	ICurrencyLookup currencyLookup,
	ISettingsService settingsService,
	IDeliveryEstimator deliveryEstimator) : ISupplierOfferConverterService
{
	public async Task<IReadOnlyList<SupplierOfferConversionResult>> ConvertAsync(
		int productId,
		string storageCode,
		IReadOnlyDictionary<Supplier, IReadOnlyList<SupplierPosition>> offers,
		CancellationToken token = default)
	{
		var results = new List<SupplierOfferConversionResult>();
		if (offers.Count == 0)
			return results;

		foreach (var (supplier, positions) in offers)
			results.Add(
				await ConvertAsync(
					productId,
					storageCode,
					supplier,
					positions,
					token));

		return results;
	}

	public async Task<SupplierOfferConversionResult> ConvertAsync(
		int productId,
		string storageCode,
		Supplier supplier,
		IReadOnlyList<SupplierPosition> positions,
		CancellationToken token = default)
	{
		var now = DateTime.UtcNow;
		var offerTtl = (await settingsService.GetOrDefault<PricingSetting>(token)).Data.OfferTtl;
		var expiresAt = now.Add(offerTtl);
		var from = DateOnly.FromDateTime(now);

		var estimates = await deliveryEstimator.EstimateAsync(
			supplier,
			storageCode,
			from,
			positions
				.Where(x => x.DeliveryInfo != null)
				.SelectMany(x => new[]
				{
					DateOnly.FromDateTime(x.DeliveryInfo!.DeliveryDate),
					DateOnly.FromDateTime(x.DeliveryInfo.GuaranteedDeliveryDate)
				}),
			token);

		var offers = new List<PriceOffer>();
		var notFoundCurrencies = new HashSet<string>();

		foreach (var supplierOffer in positions)
		{
			if (supplierOffer.PurchaseInfo == null)
				continue;
			if (supplierOffer.DeliveryInfo == null)
				continue;

			var currencyId = await currencyLookup.GetCurrencyIdAsync(
				supplierOffer.PurchaseInfo.PriceInfo.CurrencyCode,
				token);

			if (currencyId == null)
			{
				notFoundCurrencies.Add(supplierOffer.PurchaseInfo.PriceInfo.CurrencyCode);
				continue;
			}

			offers.Add(
				PriceOffer.CreateForSupplier(
					productId,
					currencyId.Value,
					storageCode,
					supplierOffer.PurchaseInfo.PriceInfo.Price,
					supplier.ToSource(),
					supplierOffer.Id,
					supplierOffer.PurchaseInfo.AvailableQuantity,
					supplierOffer.PurchaseInfo.MinimumPurchaseQuantity,
					supplierOffer.PurchaseInfo.QuantityCoefficient,
					supplierOffer.PurchaseInfo.DaysToRefund,
					estimates[DateOnly.FromDateTime(supplierOffer.DeliveryInfo.DeliveryDate)].DeliveryDays,
					estimates[DateOnly.FromDateTime(supplierOffer.DeliveryInfo.GuaranteedDeliveryDate)].DeliveryDays,
					supplierOffer.DeliveryInfo.DeliveryProbability,
					supplierOffer.DeliveryInfo.OrderTill,
					expiresAt));
		}

		return new SupplierOfferConversionResult
		{
			Supplier = supplier,
			NotFoundCurrencies = notFoundCurrencies.ToList(),
			Offers = offers
		};
	}
}
