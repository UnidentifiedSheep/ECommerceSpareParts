using Application.Common.Interfaces.Currency;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Models.Pricing;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers.Internal;

public class MinimumSupplierPriceApplier(
    ICurrencyConverter currencyConverter
    ) : ApplierNamedObjectBase, IInternalPriceApplier
{
    public override int Order => int.MinValue;
    public override string SystemName => nameof(MinimumSupplierPriceApplier);
    public override string NameLocalizationKey => "price.applier.minimum.supplier.price.name";
    public override string DescriptionLocalizationKey => "price.applier.minimum.supplier.price.description";

    public override async ValueTask<PriceCalculationState> ApplyAsync(
        PriceCalculationState state,
        CancellationToken ct = default)
    {
        if (!state.Market.HasMarket) return state;

        var referenceOffer = state.Market.Items.Count > 0 
            ? state.Market.Items[0] 
            : null;

        var salePriceInBase = await currencyConverter.ConvertToBaseAsync(
            state.SalePrice,
            state.CurrencyId,
            ct);
        
        if (referenceOffer is null || salePriceInBase >= referenceOffer.CostInBaseCurrency) 
            return state;

        var fromBase = await currencyConverter.ConvertFromBaseAsync(referenceOffer.CostInBaseCurrency, state.CurrencyId, ct);
        
        var newState = state with
        {
            SalePrice = fromBase
        };

        return newState;
    }
}
