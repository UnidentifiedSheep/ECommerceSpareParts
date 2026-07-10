using Application.Common.Abstractions.NamedObjects;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Models.Pricing;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;

public abstract class ApplierNamedObjectBase : LocalizableNameObject, IPriceApplier
{
    public abstract int Order { get; }
    public abstract ValueTask<PriceCalculationState> ApplyAsync(
        PriceCalculationState state, 
        CancellationToken ct = default);
}