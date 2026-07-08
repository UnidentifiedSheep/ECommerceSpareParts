using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Models.Pricing;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;

public sealed class MarkupApplier(
    IMarkupCalculator calculator
) : ApplierNamedObjectBase, IInternalPriceApplier, ISupplierPriceApplier
{
    public override int Order => 0;
    public override string SystemName => nameof(MarkupApplier);
    public override string NameLocalizationKey { get; }
    public override string DescriptionLocalizationKey { get; }

    public override ValueTask<PriceCalculationState> ApplyAsync(
        PriceCalculationState state,
        CancellationToken ct = default)
    {
        var markupResult = calculator.GetMarkup(state.SalePriceInBaseCurrency, state.BaseCurrencyId);
        
        var newState = state with
        {
            Markup = markupResult,
            SalePriceInBaseCurrency = markupResult.PriceInBaseCurrency,
            AppliedRules =
            [
                ..state.AppliedRules,
                new AppliedPriceRule(
                    Name: SystemName,
                    PriceBefore: state.SalePriceInBaseCurrency,
                    PriceAfter: markupResult.PriceInBaseCurrency)
            ]
        };
        
        return ValueTask.FromResult(newState);
    }
}