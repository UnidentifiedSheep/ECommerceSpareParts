using Pricing.Application.Interfaces;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Models.Pricing;

namespace Pricing.Application.Services.Markup;

public sealed class MarkupCalculator(
    IMarkupContainer markupContainer
) : IMarkupCalculator
{
    public MarkupResult GetMarkup(decimal basePrice, int currencyId)
    {
        var proportion = GetMarkupProportion(basePrice, currencyId);
        return MarkupResult.FromProportion(basePrice, proportion);
    }

    private decimal GetMarkupProportion(decimal basePrice, int currencyId)
    {
        var basePriceValue = (double)basePrice;

        return currencyId == markupContainer.DefaultCurrencyId
            ? markupContainer.GetForDefaultOrNull(basePriceValue)?.Value ?? markupContainer.DefaultMarkup.Value
            : markupContainer.GetForCurrencyOrNull(currencyId, basePriceValue)?.Value ?? markupContainer.DefaultMarkup.Value;
    }
}