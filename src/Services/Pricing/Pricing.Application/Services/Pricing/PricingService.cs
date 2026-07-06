using Application.Common.Interfaces.Currency;
using Pricing.Application.Interfaces;
using Pricing.Application.Interfaces.Markup;

namespace Pricing.Application.Services.Pricing;

public class PricingService(
    ICurrencyConverter currencyConverter,
    IMarkupCalculator markupCalculator)
{
    public async Task CalculatePriceAsync(
        int productId,
        int currencyId,
        CancellationToken token)
    {

    }
}