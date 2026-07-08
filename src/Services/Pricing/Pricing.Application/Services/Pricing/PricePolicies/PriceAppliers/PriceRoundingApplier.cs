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
    public override string NameLocalizationKey { get; }
    public override string DescriptionLocalizationKey { get; }
    public override int Order => int.MaxValue; //Rounding always the last step.
    public override async ValueTask<PriceCalculationState> ApplyAsync(
        PriceCalculationState state, 
        CancellationToken ct = default)
    {
        var roundingStep = (await settingsService.GetOrDefault<PricingSetting>(ct)).Data.PriceRoundingStep;
        var roundedPrice = Rounding.RoundToStep(state.SalePriceInBaseCurrency, roundingStep);
        return state with
        {
            SalePriceInBaseCurrency = roundedPrice,
            AppliedRules =
            [
                ..state.AppliedRules,
                new AppliedPriceRule(
                    Name: SystemName,
                    PriceBefore: state.SalePriceInBaseCurrency,
                    PriceAfter: roundedPrice)
            ]
        };
    }
}