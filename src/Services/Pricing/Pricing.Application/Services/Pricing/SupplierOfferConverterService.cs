using Application.Common.Interfaces.Settings;
using Enums;
using Integrations.Supplier.Models;
using Pricing.Application.Extensions;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models;
using Pricing.Entities;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services.Pricing;

public class SupplierOfferConverterService(
    ICachedCurrencyProvider cachedCurrencyProvider,
    ISettingsService settingsService) : ISupplierOfferConverterService
{
    public async Task<IReadOnlyList<SupplierOfferConversionResult>> ConvertAsync(
        int productId,
        string storageName,
        IReadOnlyDictionary<Supplier, IReadOnlyList<SupplierPosition>> offers,
        CancellationToken token = default)
    {
        var results = new List<SupplierOfferConversionResult>();
        if (offers.Count == 0) return results;

        foreach (var (supplier, positions) in offers)
            results.Add(await ConvertAsync(productId, storageName, supplier, positions, token));
        
        return results;
    }
    
    public async Task<SupplierOfferConversionResult> ConvertAsync(
        int productId,
        string storageName,
        Supplier supplier,
        IReadOnlyList<SupplierPosition> positions,
        CancellationToken token = default)
    {
        var offerTtl = (await settingsService.GetOrDefault<PricingSetting>(token)).Data.OfferTtl;
        var expiresAt = DateTime.UtcNow.Add(offerTtl);
        
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
                storageName,
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