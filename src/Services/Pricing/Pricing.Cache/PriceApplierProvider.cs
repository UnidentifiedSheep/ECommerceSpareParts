using Application.Common.Interfaces.Repositories;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Dtos.PriceApplier;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Projections;
using Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;
using Pricing.Application.Static;
using Pricing.Entities.Pricing;
using Pricing.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace Pricing.Cache;

public class PriceApplierProvider(
    IFusionCache cache,
    IReadRepository<PriceApplier, string> repository) : IPriceApplierProvider
{
    public async Task<IReadOnlyList<PriceApplierDto>> GetPriceAppliersAsync(
        CancellationToken ct = default)
    {
        return await cache.GetOrSetAsync(
            key: CacheKeys.PriceAppliers.Key,
            factory: GetPriceAppliersFromDbAsync,
            options: new FusionCacheEntryOptions(CacheKeys.PriceAppliers.Ttl),
            token: ct);
    }

    public async Task InvalidatePriceAppliersAsync(
        CancellationToken ct = default)
    {
        await cache.RemoveAsync(
            CacheKeys.PriceAppliers.Key, 
            token: ct);
    }

    private Task<List<PriceApplierDto>> GetPriceAppliersFromDbAsync(
        CancellationToken ct = default)
        => repository.Query
            .AsExpandable()
            .Select(PriceApplierProjections.ToApplierDto)
            .ToListAsync(ct);
}