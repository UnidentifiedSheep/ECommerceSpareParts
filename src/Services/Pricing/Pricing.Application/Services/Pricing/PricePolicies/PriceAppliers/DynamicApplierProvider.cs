using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Pricing.Entities.Pricing;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;

public class DynamicApplierProvider(
    IReadRepository<PriceApplier, string> repository)
{
    public async Task<IReadOnlyList<DynamicApplierNamedObject>> GetAppliersAsync(
        PriceApplierUsage priceApplierUsage,
        CancellationToken ct = default)
    {
        return await repository.Query
            .SelectMany(
                x => x.States.Where(z =>
                    z.Usage == priceApplierUsage && z.Enabled),
                (p, m) => new DynamicApplierNamedObject(
                    p.SystemName,
                    m.Order,
                    p.DslLogic))
            .ToListAsync(ct);
    }
}