using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Projections;
using Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;
using Pricing.Application.Static;
using Pricing.Entities.Pricing;
using ZiggyCreatures.Caching.Fusion;

namespace Pricing.Cache;

public class PriceApplierProvider(
    IFusionCache cache,
    IReadRepository<PriceApplier, string> repository,
    INamedObjectRegistry<ApplierNamedObjectBase> registry) : IPriceApplierProvider
{
    public async Task<PriceApplierConfigurationSnapshot> GetConfigurationAsync(
        CancellationToken ct = default)
    {
        return await cache.GetOrSetAsync(
            key: CacheKeys.PriceAppliers.ConfigurationKey,
            factory: GetConfigurationFromDbAsync,
            options: new FusionCacheEntryOptions(CacheKeys.PriceAppliers.Ttl),
            token: ct);
    }

    public async Task InvalidateConfigurationAsync(
        CancellationToken ct = default)
    {
        await cache.RemoveAsync(
            CacheKeys.PriceAppliers.ConfigurationKey,
            token: ct);
    }

    private async Task<PriceApplierConfigurationSnapshot> GetConfigurationFromDbAsync(
        CancellationToken ct = default)
    {
        var appliers = await repository.Query
            .AsExpandable()
            .Select(PriceApplierProjections.ToApplierDto)
            .ToListAsync(ct);

        return PriceApplierConfigurationSnapshot.Create(
            appliers,
            registry.All);
    }
}
