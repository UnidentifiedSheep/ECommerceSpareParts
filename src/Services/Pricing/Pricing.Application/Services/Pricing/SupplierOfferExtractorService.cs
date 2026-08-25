using Application.Common.Interfaces.Cache;
using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Models.Requests;
using Internal.Integration.Core.Interfaces.Main;
using Microsoft.Extensions.Logging;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models;
using Pricing.Application.Static;

namespace Pricing.Application.Services.Pricing;

public class SupplierOfferExtractorService(
    ILogger<SupplierOfferExtractorService> logger,
    IDistributedLockProvider distributedLockProvider,
    IMainClient mainClient,
    ISupplierOfferRequestMarkerService markerService,
    ISupplierFactory supplierFactory) : ISupplierOfferExtractorService
{
    public async Task<SupplierOfferExtractionResult[]> ExtractOffers(
        string storageCode,
        int productId,
        CancellationToken token = default)
    {
        var suppliers = await supplierFactory.GetAvailableSuppliers(token);
        if (suppliers.Count == 0) return [];
        
        var tasks = suppliers
            .Select(x => GetFromSupplier(x, productId, storageCode, token))
            .ToList();

        return await Task.WhenAll(tasks);
    }
    
    private async Task<SupplierOfferExtractionResult> GetFromSupplier(
        ISupplier supplier,
        int productId,
        string storageCode,
        CancellationToken token)
    {
        try
        {
            return await GetFromSupplierCore(supplier, productId, storageCode, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                exception: ex, 
                message: "Supplier offer extraction failed. Supplier: {Supplier}, ProductId: {ProductId}, Storage: {StorageCode}",
                supplier.Supplier, 
                productId,
                storageCode);
            await markerService.MarkAsFailedAsync(supplier.Supplier, productId, storageCode);
            return SupplierOfferExtractionResult.Failed(supplier.Supplier);
        }
    }

    private async Task<SupplierOfferExtractionResult> GetFromSupplierCore(
        ISupplier supplier, 
        int productId,
        string storageCode,
        CancellationToken token)
    {
        if (await markerService.HasAnyMarkerAsync(supplier.Supplier, productId, storageCode, token))
            return SupplierOfferExtractionResult.SkippedByRefreshMarker(supplier.Supplier);
        
        var result = await distributedLockProvider.TryExecuteWithLock(
            CacheKeys.Offer.Lock.Key(supplier.Supplier, productId, storageCode),
            CacheKeys.Offer.Lock.Ttl,
            async ct =>
            {
                if (await markerService.HasAnyMarkerAsync(supplier.Supplier, productId, storageCode, ct))
                    return SupplierOfferExtractionResult.SkippedByRefreshMarker(supplier.Supplier);
                
                var mainResponse = await mainClient.ProductNode
                    .GetSupplierProductReferences([productId], supplier.Supplier, ct);

                if (!mainResponse.Success || mainResponse.Value is { Count: 0 })
                {
                    await markerService.MarkAsFailedAsync(supplier.Supplier, productId, storageCode);
                    return SupplierOfferExtractionResult.NoSupplierReference(supplier.Supplier);
                }

                var reference = mainResponse.ValueOrThrow[0];
                
                var response = await supplier.GetProductsAsync(new GetProductsRequest
                {
                    StorageCode = storageCode,
                    Brand = reference.SupplierProducerName,
                    Number = reference.Sku,
                    ShowAnalogues = true
                }, ct);

                if (!response.Success || response.Value == null)
                {
                    await markerService.MarkAsFailedAsync(supplier.Supplier, productId, storageCode);
                    return SupplierOfferExtractionResult.SupplierRequestFailed(supplier.Supplier);
                }
                
                await markerService.MarkAsOkAsync(supplier.Supplier, productId, storageCode, ct);

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
}
