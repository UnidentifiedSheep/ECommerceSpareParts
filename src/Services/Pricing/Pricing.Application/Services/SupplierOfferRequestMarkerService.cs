using Application.Common.Interfaces.Cache;
using Application.Common.Interfaces.Settings;
using Enums;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Static;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services;

public class SupplierOfferRequestMarkerService(
    ISettingsService settingsService,
    ICache cache) : ISupplierOfferRequestMarkerService
{
    public async Task<bool> HasAnyMarkerAsync(
        Supplier supplier,
        int productId,
        string storageName,
        CancellationToken token)
        => (await cache.KeyExistsAsync(
                keys:
                [
                    CacheKeys.Offer.Failed.Key(supplier, productId, storageName),
                    CacheKeys.Offer.Ok.Key(supplier, productId, storageName)
                ],
                token))
            .Any(x => x.Value);

    public async Task MarkAsOkAsync(
        Supplier supplier,
        int productId,
        string storageName,
        CancellationToken token)
        => await cache.SetAsync(
            CacheKeys.Offer.Ok.Key(supplier, productId, storageName),
            true,
            CacheKeys.Offer.Ok.Ttl((await settingsService.GetOrDefault<PricingSetting>(token)).Data));

    public async Task MarkAsOkAsync(
        IEnumerable<int> productId,
        Supplier supplier,
        string storageName,
        CancellationToken token)
    {
        var keys = productId.Select(x => (
                CacheKeys.Offer.Ok.Key(supplier, x, storageName),
                true))
            .ToArray();

        if (keys.Length == 0) return;

        await cache.SetAsync(
            keys, 
            CacheKeys.Offer.Ok.Ttl((await settingsService.GetOrDefault<PricingSetting>(token)).Data));
    }
    
    public async Task MarkAsFailedAsync(
        Supplier supplier,
        int productId,
        string storageName)
        => await cache.SetAsync(
            CacheKeys.Offer.Failed.Key(supplier, productId, storageName),
            true,
            CacheKeys.Offer.Failed.Ttl);
}