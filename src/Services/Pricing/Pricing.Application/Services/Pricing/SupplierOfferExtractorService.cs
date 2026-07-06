using Application.Common.Interfaces.Cache;
using Application.Common.Interfaces.Settings;
using Enums;
using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Models.Requests;
using Internal.Integration.Core.Interfaces.Main;
using Microsoft.Extensions.Logging;
using Pricing.Application.Models;
using Pricing.Application.Static;
using Pricing.Entities.Settings;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing;

public class SupplierOfferExtractorService(
    ILogger<SupplierOfferExtractorService> logger,
    ISettingsService settingsService,
    IDistributedLockProvider distributedLockProvider,
    IMainClient mainClient,
    ICache cache,
    ISupplierFactory supplierFactory)
{
    public async Task<SupplierOfferExtractionResult[]> ExtractOffers(
        int productId,
        CancellationToken token = default)
    {
        var suppliers = await supplierFactory.GetAvailableSuppliers(token);
        if (suppliers.Count == 0) return [];
        
        var tasks = suppliers
            .Select(x => GetFromSupplier(x, productId, token))
            .ToList();

        return await Task.WhenAll(tasks);
    }
    
    private async Task<SupplierOfferExtractionResult> GetFromSupplier(
        ISupplier supplier,
        int productId,
        CancellationToken token)
    {
        try
        {
            return await GetFromSupplierCore(supplier, productId, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                exception: ex, 
                message: "Supplier offer extraction failed. Supplier: {Supplier}, ProductId: {ProductId}", 
                supplier.Supplier, 
                productId);
            await MarkAsFailed(supplier.Supplier, productId);
            return new SupplierOfferExtractionResult
            {
                Supplier = supplier.Supplier,
                Offers = [],
                Status = SupplierOfferExtractionStatus.Failed
            };
        }
    }

    private async Task<SupplierOfferExtractionResult> GetFromSupplierCore(
        ISupplier supplier, 
        int productId,
        CancellationToken token)
    {
        if (await HasReFreshMarkerAsync(supplier.Supplier, productId, token))
            return new SupplierOfferExtractionResult
            {
                Supplier = supplier.Supplier,
                Offers = [],
                Status = SupplierOfferExtractionStatus.SkippedByRefreshMarker
            };
        
        var result = await distributedLockProvider.TryExecuteWithLock(
            CacheKeys.Offer.Lock.Key(supplier.Supplier, productId),
            CacheKeys.Offer.Lock.Ttl,
            async ct =>
            {
                if (await HasReFreshMarkerAsync(supplier.Supplier, productId, ct))
                    return new SupplierOfferExtractionResult
                    {
                        Supplier = supplier.Supplier,
                        Offers = [],
                        Status = SupplierOfferExtractionStatus.SkippedByRefreshMarker
                    };
                
                var mainResponse = await mainClient.ProductNode
                    .GetSupplierProductReferences([productId], supplier.Supplier, ct);

                if (!mainResponse.Success || mainResponse.Value is { Count: 0 })
                {
                    await MarkAsFailed(supplier.Supplier, productId);
                    return new SupplierOfferExtractionResult
                    {
                        Offers = [],
                        Supplier = supplier.Supplier,
                        Status = SupplierOfferExtractionStatus.NoSupplierReference
                    };
                }

                var reference = mainResponse.ValueOrThrow[0];
                
                var response = await supplier.GetProductsAsync(new GetProductsRequest
                {
                    Brand = reference.SupplierProducerName,
                    Number = reference.Sku,
                    ShowAnalogues = true
                }, ct);

                if (!response.Success || response.Value == null)
                {
                    await MarkAsFailed(supplier.Supplier, productId);
                    return new SupplierOfferExtractionResult
                    {
                        Supplier = supplier.Supplier,
                        Offers = [],
                        Status = SupplierOfferExtractionStatus.SupplierRequestFailed
                    };
                }
                
                await MarkAsOk(supplier.Supplier, productId, ct);
                return new SupplierOfferExtractionResult
                {
                    Supplier = supplier.Supplier,
                    Offers = response.ValueOrThrow.ToList(),
                    Status = SupplierOfferExtractionStatus.Success
                };
            },
            token);

        return result ?? new SupplierOfferExtractionResult
        {
            Supplier = supplier.Supplier,
            Offers = [],
            Status = SupplierOfferExtractionStatus.AlreadyRefreshing
        };
    }

    private async Task<bool> HasReFreshMarkerAsync(
        Supplier supplier,
        int productId,
        CancellationToken token)
        => (await cache.KeyExistsAsync(
                keys:
                [
                    CacheKeys.Offer.Failed.Key(supplier, productId),
                    CacheKeys.Offer.Ok.Key(supplier, productId)
                ],
                token))
            .Any(x => x.Value);

    private async Task MarkAsOk(
        Supplier supplier,
        int productId,
        CancellationToken token)
        => await cache.SetAsync(
            CacheKeys.Offer.Ok.Key(supplier, productId),
            true,
            CacheKeys.Offer.Ok.Ttl((await settingsService.GetOrDefault<PricingSetting>(token)).Data));

    private async Task MarkAsFailed(
        Supplier supplier,
        int productId)
        => await cache.SetAsync(
            CacheKeys.Offer.Failed.Key(supplier, productId),
            true,
            CacheKeys.Offer.Failed.Ttl);
}