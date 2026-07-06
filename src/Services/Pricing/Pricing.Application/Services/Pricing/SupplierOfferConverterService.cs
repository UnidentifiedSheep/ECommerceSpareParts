using Enums;
using Integrations.Supplier.Models;
using Pricing.Application.Extensions;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models;
using Pricing.Entities;

namespace Pricing.Application.Services.Pricing;

public class SupplierOfferConverterService(
    ICachedCurrencyProvider cachedCurrencyProvider) : ISupplierOfferConverterService
{
    public async Task<IReadOnlyList<SupplierOfferConversionResult>> ConvertAsync(
        int productId,
        IReadOnlyDictionary<Supplier, IReadOnlyList<SupplierPosition>> offers,
        CancellationToken token = default)
    {
        var results = new List<SupplierOfferConversionResult>();
        if (offers.Count == 0) return results;

        foreach (var (supplier, positions) in offers)
            results.Add(await ConvertAsync(productId, supplier, positions, token));
        
        return results;
    }
    
    public async Task<SupplierOfferConversionResult> ConvertAsync(
        int productId,
        Supplier supplier,
        IReadOnlyList<SupplierPosition> positions,
        CancellationToken token = default)
    {
        var offers = new List<PriceOffer>();
        var notFoundCurrencies = new HashSet<string>();
        
        foreach (var supplierOffer in positions)
        {
            if (supplierOffer.PurchaseInfo == null) continue;
            if (supplierOffer.DeliveryInfo == null) continue;
                
            var currencyId = await cachedCurrencyProvider
                .GetCurrencyIdAsync(supplierOffer.PurchaseInfo.PriceInfo.CurrencyCode, token);

            if (currencyId == null)
            {
                notFoundCurrencies.Add(supplierOffer.PurchaseInfo.PriceInfo.CurrencyCode);
                continue;
            }
                
            offers.Add(PriceOffer.Create(
                productId,
                currencyId.Value,
                supplierOffer.PurchaseInfo.PriceInfo.Price,
                supplier.ToSource(),
                supplierOffer.Id,
                supplierOffer.PurchaseInfo.AvailableQuantity,
                supplierOffer.PurchaseInfo.MinimumPurchaseQuantity,
                supplierOffer.PurchaseInfo.QuantityCoefficient,
                supplierOffer.PurchaseInfo.DaysToRefund,
                supplierOffer.DeliveryInfo.DeliveryDate,
                supplierOffer.DeliveryInfo.GuaranteedDeliveryDate,
                supplierOffer.DeliveryInfo.DeliveryProbability,
                supplierOffer.DeliveryInfo.OrderTill,
                DateTime.UtcNow.AddDays(1))); //TODO this should be taken from settings, cuz depends on supplier
        }

        return new SupplierOfferConversionResult
        {
            Supplier = supplier,
            NotFoundCurrencies = notFoundCurrencies.ToList(),
            Offers = offers
        };
    }
}