using Application.Common.Interfaces.Settings;
using Extensions;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Models.Pricing;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;

public class PriceRoundingApplier(
    ISettingsService settingsService
    ) : ApplierNamedObjectBase, IInternalPriceApplier, ISupplierPriceApplier
{
    public override string SystemName => nameof(PriceRoundingApplier);
    public override string NameLocalizationKey => "price.applier.price.rounding.name";
    public override string DescriptionLocalizationKey => "price.applier.price.rounding.description";
    public override int Order => 100000;
    public override async ValueTask<PriceCalculationState> ApplyAsync(
        PriceCalculationState state, 
        CancellationToken ct = default)
    {
        var roundingStep = (await settingsService.GetOrDefault<PricingSetting>(ct)).Data.PriceRoundingStep;
        var roundedPrice = Rounding.RoundToStep(state.SalePrice, roundingStep);
        return state with
        {
            SalePrice = roundedPrice,
            AppliedRules =
            [
                ..state.AppliedRules,
                new AppliedPriceRule(
                    Name: SystemName,
                    PriceBefore: state.SalePrice,
                    PriceAfter: roundedPrice)
            ]
        };
    }
}
