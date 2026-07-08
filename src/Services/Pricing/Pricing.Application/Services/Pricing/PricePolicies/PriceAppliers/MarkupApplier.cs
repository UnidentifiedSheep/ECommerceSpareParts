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
    public override string NameLocalizationKey => "price.applier.markup.name";
    public override string DescriptionLocalizationKey => "price.applier.markup.description";

    public override ValueTask<PriceCalculationState> ApplyAsync(
        PriceCalculationState state,
        CancellationToken ct = default)
    {
        var markupResult = calculator.GetMarkup(state.SalePriceInBaseCurrency, state.BaseCurrencyId);
        
        var newState = state with
        {
            BaseMarkup = markupResult,
            SalePriceInBaseCurrency = markupResult.ResultingPrice,
            AppliedRules =
            [
                ..state.AppliedRules,
                new AppliedPriceRule(
                    Name: SystemName,
                    PriceBefore: state.SalePriceInBaseCurrency,
                    PriceAfter: markupResult.ResultingPrice)
            ]
        };
        
        return ValueTask.FromResult(newState);
    }
}
