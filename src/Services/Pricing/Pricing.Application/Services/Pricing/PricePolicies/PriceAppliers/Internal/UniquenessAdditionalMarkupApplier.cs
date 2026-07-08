using Application.Common.Interfaces.Settings;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Models.Pricing;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers.Internal;

public class UniquenessAdditionalMarkupApplier(
    ISettingsService settingsService
    ) : ApplierNamedObjectBase, IInternalPriceApplier
{
    public override int Order => 1;
    public override string SystemName => nameof(UniquenessAdditionalMarkupApplier);
    public override string NameLocalizationKey => "price.applier.uniqueness.additional.markup.name";
    public override string DescriptionLocalizationKey => "price.applier.uniqueness.additional.markup.description";
    public override async ValueTask<PriceCalculationState> ApplyAsync(
        PriceCalculationState state, 
        CancellationToken ct = default)
    {
        if (state.Market.HasMarket) return state;
        
        var priceSettings = (await settingsService.GetOrDefault<PricingSetting>(ct)).Data;
        var applied = state.SalePriceInBaseCurrency * (1 + priceSettings.UniqProductAdditionalMarkup);
        
        return state with
        {
            SalePriceInBaseCurrency = applied,
            AppliedRules =
            [
                ..state.AppliedRules,
                new AppliedPriceRule(
                    Name: SystemName,
                    PriceBefore: state.SalePriceInBaseCurrency,
                    PriceAfter: applied)
            ]
        };
    }
}
