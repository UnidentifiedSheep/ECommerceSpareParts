using Application.Common.Interfaces.Cache;
using Application.Common.Interfaces.Settings;
using Enums;
using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Models.Requests;
using Internal.Integration.Core.Interfaces.Main;
using Microsoft.Extensions.Logging;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models;
using Pricing.Application.Static;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services.Pricing;

public class SupplierOfferExtractorService(
    ILogger<SupplierOfferExtractorService> logger,
    ISettingsService settingsService,
    IDistributedLockProvider distributedLockProvider,
    IMainClient mainClient,
    ICache cache,
    ISupplierFactory supplierFactory) : ISupplierOfferExtractorService
{
    public async Task<SupplierOfferExtractionResult[]> ExtractOffers(
        string storageName,
        int productId,
        CancellationToken token = default)
    {
        var suppliers = await supplierFactory.GetAvailableSuppliers(token);
        if (suppliers.Count == 0) return [];
        
        var tasks = suppliers
            .Select(x => GetFromSupplier(x, productId, storageName, token))
            .ToList();

        return await Task.WhenAll(tasks);
    }
    
    private async Task<SupplierOfferExtractionResult> GetFromSupplier(
        ISupplier supplier,
        int productId,
        string storageName,
        CancellationToken token)
    {
        try
        {
            return await GetFromSupplierCore(supplier, productId, storageName, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                exception: ex, 
                message: "Supplier offer extraction failed. Supplier: {Supplier}, ProductId: {ProductId}, Storage: {StorageName}", 
                supplier.Supplier, 
                productId,
                storageName);
            await MarkAsFailed(supplier.Supplier, productId, storageName);
            return SupplierOfferExtractionResult.Failed(supplier.Supplier);
        }
    }

    private async Task<SupplierOfferExtractionResult> GetFromSupplierCore(
        ISupplier supplier, 
        int productId,
        string storageName,
        CancellationToken token)
    {
        if (await HasReFreshMarkerAsync(supplier.Supplier, productId, storageName, token))
            return SupplierOfferExtractionResult.SkippedByRefreshMarker(supplier.Supplier);
        
        var result = await distributedLockProvider.TryExecuteWithLock(
            CacheKeys.Offer.Lock.Key(supplier.Supplier, productId, storageName),
            CacheKeys.Offer.Lock.Ttl,
            async ct =>
            {
                if (await HasReFreshMarkerAsync(supplier.Supplier, productId, storageName, ct))
                    return SupplierOfferExtractionResult.SkippedByRefreshMarker(supplier.Supplier);
                
                var mainResponse = await mainClient.ProductNode
                    .GetSupplierProductReferences([productId], supplier.Supplier, ct);

                if (!mainResponse.Success || mainResponse.Value is { Count: 0 })
                {
                    await MarkAsFailed(supplier.Supplier, productId, storageName);
                    return SupplierOfferExtractionResult.NoSupplierReference(supplier.Supplier);
                }

                var reference = mainResponse.ValueOrThrow[0];
                
                var response = await supplier.GetProductsAsync(new GetProductsRequest
                {
                    StorageName = storageName,
                    Brand = reference.SupplierProducerName,
                    Number = reference.Sku,
                    ShowAnalogues = true
                }, ct);

                if (!response.Success || response.Value == null)
                {
                    await MarkAsFailed(supplier.Supplier, productId, storageName);
                    return SupplierOfferExtractionResult.SupplierRequestFailed(supplier.Supplier);
                }
                
                await MarkAsOk(supplier.Supplier, productId, storageName, ct);

                return response.ValueOrThrow.Count switch
                {
                    1 => SupplierOfferExtractionResult.Success(supplier.Supplier, response.ValueOrThrow[0]),
                    0 => SupplierOfferExtractionResult.SupplierReturnedEmpty(supplier.Supplier),
                    _ => SupplierOfferExtractionResult.InvalidSupplierResponse(supplier.Supplier)
                };
            },
            token);

        return result ?? SupplierOfferExtractionResult.AlreadyRefreshing(supplier.Supplier);
    }

    private async Task<bool> HasReFreshMarkerAsync(
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

    private async Task MarkAsOk(
        Supplier supplier,
        int productId,
        string storageName,
        CancellationToken token)
        => await cache.SetAsync(
            CacheKeys.Offer.Ok.Key(supplier, productId, storageName),
            true,
            CacheKeys.Offer.Ok.Ttl((await settingsService.GetOrDefault<PricingSetting>(token)).Data));

    private async Task MarkAsFailed(
        Supplier supplier,
        int productId,
        string storageName)
        => await cache.SetAsync(
            CacheKeys.Offer.Failed.Key(supplier, productId, storageName),
            true,
            CacheKeys.Offer.Failed.Ttl);
}