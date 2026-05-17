using IntervalMap.Variations;
using Pricing.Application.Interfaces.Services.Pricing;
using Pricing.Entities;

namespace Pricing.Application.Services.ProductPricing;

public class MarkupService : IMarkupService
{
    public Task SetUp(MarkupGroup markupGroup)
    {
        throw new NotImplementedException();
    }

    public decimal GetMarkup(decimal value, int currencyId)
    {
        throw new NotImplementedException();
    }

    public decimal WithMarkup(decimal value, decimal markupFraction)
    {
        throw new NotImplementedException();
    }

    private record Markup(decimal Value);
}