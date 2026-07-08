using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Models.Pricing;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers.Internal;

public class MinimumSupplierPriceApplier : ApplierNamedObjectBase, IInternalPriceApplier
{
    public override int Order => int.MinValue;
    public override string SystemName => nameof(MinimumSupplierPriceApplier);
    public override string NameLocalizationKey { get; }
    public override string DescriptionLocalizationKey { get; }

    public override ValueTask<PriceCalculationState> ApplyAsync(
        PriceCalculationState state,
        CancellationToken ct = default)
    {
        if (!state.Market.HasMarket) return ValueTask.FromResult(state);

        var referenceOffer = state.Market.Items.Count > 0 
            ? state.Market.Items[0] 
            : null;

        if (referenceOffer is null || state.SalePriceInBaseCurrency >= referenceOffer.Cost) 
            return ValueTask.FromResult(state);

        var newState = state with
        {
            SalePriceInBaseCurrency = referenceOffer.Cost
        };

        return ValueTask.FromResult(newState);
    }
}