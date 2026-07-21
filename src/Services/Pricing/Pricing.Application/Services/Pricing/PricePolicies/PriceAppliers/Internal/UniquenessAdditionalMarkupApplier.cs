using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Settings;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Models.Pricing;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers.Internal;

public class UniquenessAdditionalMarkupApplier(
    ISettingsService settingsService
    ) : ApplierNamedObjectBase, IInternalPriceApplier
{
    public override int Order => 10000;
    public override string SystemName => nameof(UniquenessAdditionalMarkupApplier);
    public override string NameLocalizationKey => "price.applier.uniqueness.additional.markup.name";
    public override string DescriptionLocalizationKey => "price.applier.uniqueness.additional.markup.description";
    public override async ValueTask<PriceCalculationState> ApplyAsync(
        PriceCalculationState state, 
        CancellationToken ct = default)
    {
        if (state.Market.HasMarket) return state;
        
        var priceSettings = (await settingsService.GetOrDefault<PricingSetting>(ct)).Data;
        var applied = state.SalePrice * (1 + priceSettings.UniqProductAdditionalMarkup);
        
        return state with
        {
            SalePrice = applied,
            AppliedRules =
            [
                ..state.AppliedRules,
                new AppliedPriceRule(
                    Name: SystemName,
                    PriceBefore: state.SalePrice,
                    PriceAfter: applied)
            ]
        };
    }
}
