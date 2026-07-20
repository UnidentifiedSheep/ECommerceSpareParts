using Application.Common.Interfaces.NamedObject;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;

public class PriceApplierService(
    IPriceApplierProvider priceApplierProvider,
    INamedObjectRegistry<ApplierNamedObjectBase> registry) : IPriceApplierService
{
    public async Task<IReadOnlyList<IPriceApplier>> GetPriceAppliersAsync(
        PriceOfferSourceType usage, 
        CancellationToken ct = default)
    {
        var allLocal = registry.All
            .Where(x => usage switch
            {
                PriceOfferSourceType.OurWarehouse => x is IInternalPriceApplier,
                PriceOfferSourceType.Supplier => x is ISupplierPriceApplier,
                _ => false
            });
        var appliers = (await priceApplierProvider
            .GetPriceAppliersAsync(ct))
            .ToDictionary(x => x.SystemName);

        var result = new List<IPriceApplier>();
        var localSystemNames = new HashSet<string>();
        foreach (var local in allLocal)
        {
            localSystemNames.Add(local.SystemName);
            if (!appliers.TryGetValue(local.SystemName, out var applier))
            {
                result.Add(local);
                continue;
            }

            var state = applier.States.FirstOrDefault(z => z.Usage == usage);
            if (state is not { Enabled: true }) continue;
            result.Add(local);
        }

        foreach (var (systemName, applier) in appliers)
        {
            if (localSystemNames.Contains(systemName) || !applier.IsDynamic) continue;
            var state = applier.States
                .FirstOrDefault(x => x.Usage == usage);
            if (state == null || !state.Enabled || applier.DslLogic is null) continue;
            result.Add(new DynamicApplierNamedObject(
                systemName,
                state.Order,
                applier.DslLogic));
        }
        
        return result.OrderBy(x => x.Order).ToList();
    }
}
