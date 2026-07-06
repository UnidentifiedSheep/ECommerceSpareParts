using Pricing.Application.Interfaces;
using Pricing.Application.Interfaces.Markup;

namespace Pricing.Application.Services.Markup;

public class MarkupCalculator(
    IMarkupContainer container
) : IMarkupCalculator
{
    public decimal GetMarkup(
        decimal basePrice,
        int currencyId)
    {
        if (currencyId == container.DefaultCurrencyId)
            return container.GetForDefaultOrNull((double)basePrice)?.Value ?? container.DefaultMarkup.Value;

        return container.GetForCurrencyOrNull(currencyId, (double)basePrice)?.Value ??
               container.DefaultMarkup.Value;
    }
}