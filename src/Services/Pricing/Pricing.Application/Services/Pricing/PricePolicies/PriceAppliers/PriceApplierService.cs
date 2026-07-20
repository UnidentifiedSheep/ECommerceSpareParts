using Application.Common.Interfaces.NamedObject;
using Localization.Abstractions.Interfaces;
using Pricing.Application.Dtos.PriceApplier;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;

public class PriceApplierService(
    IPriceApplierProvider priceApplierProvider,
    INamedObjectRegistry<ApplierNamedObjectBase> registry,
    IScopedStringLocalizer localizer) : IPriceApplierService
{
    public async Task<IReadOnlyList<PriceApplierDto>> GetPriceApplierInfosAsync(
        PriceOfferSourceType usage,
        CancellationToken ct = default)
    {
        var registeredLocals = registry.All.ToList();
        var localSystemNames = registeredLocals
            .Select(x => x.SystemName)
            .ToHashSet();
        var persistedAppliers = (await priceApplierProvider
                .GetPriceAppliersAsync(ct))
            .ToDictionary(x => x.SystemName);
        var result = new List<PriceApplierDto>();

        foreach (var local in registeredLocals
                     .Where(x => SupportsUsage(x, usage)))
        {
            if (!persistedAppliers.TryGetValue(local.SystemName, out var persisted))
            {
                result.Add(CreateLocalDto(local, usage, true));
                continue;
            }

            var state = persisted.States.FirstOrDefault(x => x.Usage == usage) is { } persistedState
                ? persistedState with { Order = local.Order }
                : CreateState(local.SystemName, usage, local.Order, false);
            result.Add(persisted with
            {
                Name = local.GetLocalizedName(localizer),
                IsDynamic = false,
                DslLogic = null,
                States = [state]
            });
        }

        foreach (var persisted in persistedAppliers.Values.Where(x =>
                     x.IsDynamic && !localSystemNames.Contains(x.SystemName)))
        {
            var state = persisted.States.FirstOrDefault(x => x.Usage == usage);
            if (state is null) continue;

            result.Add(persisted with { States = [state] });
        }

        return result
            .OrderBy(x => x.States[0].Order)
            .ThenBy(x => x.SystemName)
            .ToList();
    }

    public async Task<IReadOnlyList<IPriceApplier>> GetPriceAppliersAsync(
        PriceOfferSourceType usage,
        CancellationToken ct = default)
    {
        var infos = await GetPriceApplierInfosAsync(usage, ct);
        var registeredLocals = registry.All.ToDictionary(x => x.SystemName);
        var result = new List<IPriceApplier>();

        foreach (var info in infos.Where(x => x.States[0].Enabled))
        {
            if (registeredLocals.TryGetValue(info.SystemName, out var local))
            {
                result.Add(local);
                continue;
            }

            if (!info.IsDynamic || info.DslLogic is null) continue;
            var state = info.States[0];
            result.Add(new DynamicApplierNamedObject(
                info.SystemName,
                state.Order,
                info.DslLogic));
        }
        
        return result.OrderBy(x => x.Order).ToList();
    }

    private PriceApplierDto CreateLocalDto(
        ApplierNamedObjectBase local,
        PriceOfferSourceType usage,
        bool enabled)
    {
        return new PriceApplierDto
        {
            SystemName = local.SystemName,
            Name = local.GetLocalizedName(localizer),
            IsDynamic = false,
            DslLogic = null,
            States = [CreateState(local.SystemName, usage, local.Order, enabled)]
        };
    }

    private static PriceApplierStateDto CreateState(
        string systemName,
        PriceOfferSourceType usage,
        int order,
        bool enabled)
    {
        return new PriceApplierStateDto
        {
            PriceApplierSystemName = systemName,
            Usage = usage,
            Order = order,
            Enabled = enabled
        };
    }

    private static bool SupportsUsage(
        ApplierNamedObjectBase local,
        PriceOfferSourceType usage)
    {
        return usage switch
        {
            PriceOfferSourceType.OurWarehouse => local is IInternalPriceApplier,
            PriceOfferSourceType.Supplier => local is ISupplierPriceApplier,
            _ => false
        };
    }
}
