using System.Net;
using Application.Common.Interfaces.Repositories;
using Exceptions;
using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Models.Requests;
using Internal.Integration.Core.Interfaces.Main;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Interfaces.Cache;
using Pricing.Entities;

namespace Pricing.Application.Services.Pricing;

public class PriceOffersExtractor(
    IReadRepository<PriceOffer, Guid> offerRepository,
    ISupplierFactory supplierFactory,
    IPricingCacheRepository pricingCacheRepository,
    IMainClient mainClient)
{
    public async Task<IReadOnlyList<PriceOffer>> GetOffersAsync(
        int productId,
        CancellationToken token)
    {
        var offers = await GetAvailableOffersFromDb(productId, token);
        throw new NotImplementedException();
    }

    private async Task<IReadOnlyList<PriceOffer>> GetAvailableOffersFromDb(
        int productId,
        CancellationToken token)
    {
        var now = DateTime.UtcNow;
        return await offerRepository.Query
            .Where(x => x.ProductId == productId)
            .Where(x => x.AvailableQuantity > 0)
            .Where(x => x.ExpiresAt >= now)
            .ToListAsync(token);
    }

    private async Task<IReadOnlyList<PriceOffer>> GetOffersFromSuppliers(
        int productId,
        CancellationToken token)
    {
        var product = await mainClient.ProductNode
            .GetFullProduct([productId], token);

        if (product.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidInputException(""); //return to user.

        if (!product.Success)
            throw new InvalidOperationException("Unable to fetch product from main service.");

        var suppliers = await GetSuppliers(productId, token);
        if (suppliers.Count == 0) return [];

        var results = await Task.WhenAll(
            suppliers.Select(s => s.GetProductsAsync(
                new GetProductsRequest
                {
                    Brand = product.ValueOrThrow[0].ProducerName,
                    Number = product.ValueOrThrow[0].Sku,
                    ShowAnalogues = true
                },
                token)));
        throw new NotImplementedException();
    }

    private async Task<IReadOnlyList<ISupplier>> GetSuppliers(
        int productId,
        CancellationToken token)
    {
        var suppliers = await supplierFactory.GetAvailableSuppliers(token);
        var locks = suppliers
            .Select(supplier =>
            {
                var lockKey = Guid.NewGuid().ToString("N");
                return (
                    lockKey,
                    supplier,
                    task: pricingCacheRepository.TryLockSupplierRequestAsync(
                        productId,
                        supplier.Supplier,
                        lockKey,
                        token));
            })
            .ToList();
        var results = await Task.WhenAll(locks.Select(x => x.task));
        return locks
            .Where((item, i) => results[i] == item.lockKey)
            .Select(x => x.supplier)
            .ToList();
    }
}